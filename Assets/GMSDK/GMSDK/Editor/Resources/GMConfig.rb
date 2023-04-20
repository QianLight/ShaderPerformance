require 'Xcodeproj'
# require 'find'
require 'json'
require 'pathname'

STDOUT.sync = true
STDERR.sync = true

class GMConfig
    HELP_STRING = '
本脚本可以将从字节官网下载Framework、Bundle自动导入游戏工程。本脚本需要与xcodeproj文件同级目录。
参数列表：
[-project_path] [必填] 游戏工程的pbxproj文件路径
[-project_target_name] [必填] 游戏工程的主要target名称
[-dynamic_target_name] [选填] 如果你的工程是先通过集成动态库再将产物打包集成主工程，那么这里需要填写你的动态库target名称，如Unity 2019.3+的版本
[-ios_gsdk_home] [选填] 存放GSDK的文件夹路径，默认为脚本同级目录下的GMSDK文件夹中
'

    attr_accessor :project_path
    attr_accessor :main_target_name
    attr_accessor :dynamic_target_name
    attr_accessor :ios_gsdk_home
    attr_accessor :framework_folderpath

    def initialize()
        # self.project_path = '/Users/Cliffe/Code/iOS/GameDemo/GameDemo.xcodeproj'
        # self.main_target_name = 'GameDemo'
        # self.dynamic_target_name = 'GameDynamic'
        # self.ios_gsdk_home = nil
        # self.framework_folderpath = nil    
        self.parseArguments()
        self.prepare()
    end

    def help_string()
        HELP_STRING
    end

    def parseArguments()
        def getValue(index)
            if ARGV.length <= index + 1 or ARGV[index + 1].start_with?("-")
                puts help_string
                exit(1)
            end
            ARGV[index + 1]
        end
        ARGV.each_with_index {|name, index|
            case name
            when '-project_path'
                self.project_path = getValue(index)
            when '-project_target_name'
                self.main_target_name = getValue(index)
            when '-dynamic_target_name'
                self.dynamic_target_name = getValue(index)
            when '-ios_gsdk_home'
                self.ios_gsdk_home = getValue(index)
            when '-help'
                puts help_string
                exit(0)
            end
        }
    end

    def prepare()
        self.ios_gsdk_home = 'GMSDK/' if !ios_gsdk_home

        if !project_path or !main_target_name or !File::exist?(project_path)
            puts "确认project_path与main_target_name参数"
            puts help_string
            exit(1)
        end
    
        if Pathname.new(File.dirname(__FILE__)).realpath != Pathname.new(File.dirname(project_path)).realpath
            puts "本脚本与xcodeproj文件不在同级目录下"
            puts help_string
            exit(1)
        end
    
        #project_path设为绝对路径
        self.project_path = Pathname.new(project_path).realpath.to_s
    
        if !File::exist?(ios_gsdk_home)
            puts "SDK目录不存在"
            puts help_string
            exit(1)
        end
    
        self.ios_gsdk_home = Pathname.new(ios_gsdk_home).realpath.to_s
    
        if !ios_gsdk_home.start_with?(Pathname.new(File.dirname(__FILE__)).realpath.to_s)
            puts "SDK目录的位置不正确，层级不能高于xcodeproj目录"
            puts help_string
            exit(1)
        end
    
        self.ios_gsdk_home = ios_gsdk_home[Pathname.new(File.dirname(__FILE__)).realpath.to_s.length + 1 .. ios_gsdk_home.length]
    
        #cd到root目录
        srcroot = Pathname.new(File.dirname(__FILE__)).realpath
        Dir.chdir srcroot
    
        if !File::exist?("#{ios_gsdk_home}/GMSDKConfig.json")
            puts "GMSDKConfig.json配置文件不存在"
            puts help_string
            exit(1)
        end
    
        self.framework_folderpath = ios_gsdk_home + "/gsdk"
    
    end

    def integrate()
        project = Xcodeproj::Project.open(project_path)

        main_target = nil
        dynamic_target = nil
        project.targets.each do |target|
            if target.name == main_target_name
                main_target = target
            end
            if dynamic_target_name and target.name == dynamic_target_name
                dynamic_target = target
            end
        end

        #移除SDK
        ios_home_group = project.main_group.find_subpath('GMSDK')
        if ios_home_group
            puts '移除已有GMSDK'
            remove_group(project, ios_home_group)
        end

        #添加SDK
        ios_home_group = project.main_group.find_subpath('GMSDK', true)
        gsdk_group = project.main_group.find_subpath('GMSDK/gsdk', true)

        #hardcode 先添加主包，不然会有编译问题
        if File::exist?("#{framework_folderpath}/BD_GameSDK_iOS.framework")
            add_custom_framework_entry(main_target, dynamic_target, project, "#{framework_folderpath}/BD_GameSDK_iOS.framework", gsdk_group)
        end

        # 添加framework
        Dir.foreach("#{framework_folderpath}/") do |file|
            if file.end_with?(".framework")
                add_custom_framework_entry(main_target, dynamic_target, project, "#{framework_folderpath}/#{file}", gsdk_group)
            end
        end

        # 添加资源
        resource_list_path = "#{framework_folderpath}/gsdk_resources.txt"
        if File.exist?(resource_list_path)
            contents = File.read(resource_list_path)
            contents.split("\n").each do |line|
                res = line.strip
                add_resource_to_group(main_target, project, "#{framework_folderpath}/#{res}", gsdk_group)
            end
        else
            Dir.foreach("#{framework_folderpath}/") do |file|
                if file.end_with?(".bundle")
                    add_bundle_to_group(main_target, project, "#{framework_folderpath}/#{file}", gsdk_group)
                elsif file.end_with?(".metallib")
                    add_bundle_to_group(main_target, project, "#{framework_folderpath}/#{file}", gsdk_group)
                end
            end
        end

        #添加系统库
        configJson = File.read("#{ios_gsdk_home}/GMSDKConfig.json")
        configObj = JSON.parse(configJson)
        configObj["SystemFrameworks"].each do |system_framework|
            puts "添加系统库:#{system_framework}"
            add_system_framework(main_target, project, system_framework)
            if dynamic_target
                add_system_framework(dynamic_target, project, system_framework)
            end
        end

        #添加系统库(optional)
        configObj["WeakSystemFrameworks"].each do |weak_system_framework|
            puts "添加系统库optional:#{weak_system_framework}"
            add_system_framework(main_target, project, weak_system_framework, true)
            if dynamic_target
                add_system_framework(dynamic_target, project, weak_system_framework, true)
            end
        end

        #脚本
        remove_all_shell_script(main_target, project);
        configObj["ShellPhase"].each do |shell_phase|
            add_shell_script_to_target(main_target, project, shell_phase["name"], shell_phase["script"], shell_phase["top"])
        end

        #BuildSettings
        modify_buildsettings(main_target, project, "$(PROJECT_DIR)/#{framework_folderpath}")
        if dynamic_target
            modify_buildsettings(dynamic_target, project,  "$(PROJECT_DIR)/#{framework_folderpath}")
        end

        #expose header files
        if configObj["ExposeHeaders"]
            configObj["ExposeHeaders"].each do |expose|
                targetName = expose["targetName"];
                target = project.targets.detect { |target| target.name == targetName }
                
                if !target
                    puts "不存在targetName:#{targetName}"
                    next
                end
                expose_public_headers(target, project, [expose["headerFile"]])
            end
        end


        project.save

        puts "GMConfig project success"
    end

    def is_dynamic_framework(framework_path)
        #TODO: 路径（相对路径）
        extn = File.extname framework_path
        filename = File.basename framework_path, extn
        ret = `file #{framework_path}/#{filename}`
        if ret.include?("dynamically linked shared library")
            return true
        end
        return false
    end

    def add_custom_framework(target, project, framework_path, to_group, embed=false)
        if to_group and File::exist?(framework_path) then
            framework_reference = to_group.find_file_by_path(framework_path)
            if !framework_reference
                framework_reference = to_group.new_reference(framework_path)
            end

            #添加进去
            build_phase = target.frameworks_build_phase
            file_ref = framework_reference
            file_ref.name = File::basename(framework_path)
            file_ref.source_tree = 'SOURCE_ROOT'
            build_file = build_phase.add_file_reference(file_ref, true)

            if !embed
                return
            end

            # 如果framework是动态库，还需要把它添加到 Embed Frameworks 中：
            # 先找到 Embed Frameworks 对应的group（新创建的项目可能没有这一项，需要先手动添加 Embed Frameworks 这个选项）
            embed_frameworks_group = nil
            target.copy_files_build_phases.each do |e|
                if e.display_name.end_with?("Embed Frameworks")
                    embed_frameworks_group = e
                    break
                end
            end
            # 如果找到 Embed Frameworks ，添加索引
            if !embed_frameworks_group
                embed_frameworks_group = project.new(Xcodeproj::Project::Object::PBXCopyFilesBuildPhase)
                embed_frameworks_group.name = 'Embed Frameworks'
                embed_frameworks_group.symbol_dst_subfolder_spec = :frameworks
                target.build_phases << embed_frameworks_group
            end

            embed_file = embed_frameworks_group.add_file_reference(file_ref, true)
            embed_file.settings = { 'ATTRIBUTES' => ["CodeSignOnCopy", "RemoveHeadersOnCopy"] }

        end
    end

    #添加所有系统库：framework、tbd、dylib
    def add_system_framework(target, project, framework_name, optional = false)
        if framework_name
            if framework_name.end_with?(".framework")
                gm_add_system_frameworks(target, project, [framework_name[0,framework_name.length - '.framework'.length]], optional)
            elsif framework_name.end_with?(".tbd")
                if framework_name.start_with?("lib")
                    target.add_system_library_tbd(framework_name[('lib'.length) .. (framework_name.length - '.tbd'.length - 1)])
                else
                    target.add_system_library_tbd(framework_name[0,framework_name.length - '.tbd'.length])
                end
                
            elsif framework_name.end_with?(".dylib")
                if framework_name.start_with?("lib")
                    target.add_system_library(framework_name[('lib'.length) .. (framework_name.length - '.dylib'.length - 1)])
                else
                    target.add_system_library(framework_name[0,framework_name.length - '.dylib'.length])
                end
            end
        end

    end

    #添加系统的framework库
    def exist_framework(build_phase, name)
        build_phase.files.each do |file|
            if file.file_ref.name == "#{name}.framework"
                return true
            end
        end
        return false
    end

    def framework_ref(project, name)
        project.targets.each do |target|
            build_phase = target.frameworks_build_phase
            build_phase.files.each do |file|
                if file.file_ref.name == "#{name}.framework"
                    return file.file_ref
                end
            end
        end
        return nil
    end

    #添加系统的framework库
    def gm_add_system_frameworks(target, project, names, optional = false)
        build_phase = target.frameworks_build_phase
        framework_group = project.frameworks_group

        names.each do |name|
            if exist_framework(build_phase, name)
                next
            end
            ref = framework_ref(project, name)
            if ref
                build_file = build_phase.add_file_reference(ref, true)
            else
                path = "System/Library/Frameworks/#{name}.framework"
                file_ref = framework_group.new_reference(path)
                file_ref.name = "#{name}.framework"
                file_ref.source_tree = 'SDKROOT'
                build_file = build_phase.add_file_reference(file_ref, true)
            end
            
            if optional
                build_file.settings = { 'ATTRIBUTES' => ['Weak'] }
            end
        end
    end

    def add_bundle_to_group(target, project, bundle_path, to_group)
        if to_group and File::exist?(bundle_path) then
            bundle_reference = to_group.find_file_by_path(bundle_path)
            if !bundle_reference
                bundle_reference = to_group.new_reference(bundle_path)
            end
            puts "添加Bundle: #{bundle_path}"
            bundle_reference.name = File::basename(bundle_path)
            bundle_reference.source_tree = 'SOURCE_ROOT'
            target.resources_build_phase.add_file_reference(bundle_reference, true)
        end
    end

    def add_resource_to_group(target, project, res_path, to_group)
        if to_group and File::exist?(res_path) then
            bundle_reference = to_group.find_file_by_path(res_path)
            if !bundle_reference
                bundle_reference = to_group.new_reference(res_path)
            end
            puts "添加resource: #{res_path}"
            bundle_reference.name = File::basename(res_path)
            bundle_reference.source_tree = 'SOURCE_ROOT'
            target.resources_build_phase.add_file_reference(bundle_reference, true)
        end
    end

    def remove_all_shell_script(target, project)
        delete_array=[]
        target.build_phases.each do |phase|
            if phase.class.name.include?("PBXShellScriptBuildPhase")
                if phase.name and phase.name.start_with?("GMShell")
                    delete_array.push(phase);
                end
            end
        end
        if delete_array
            delete_array.each do |phase|
                puts "移除已有Shell:#{phase.name}"
                target.build_phases.delete(phase);
            end
        end
    end

    def add_shell_script_to_target(target, project, shell_name, shell_script, top = false)
        puts "添加Shell: #{shell_name}"
        uuid = rand(10**(24-1)..10**(24)).to_s
        run_script_phase = Xcodeproj::Project::Object::PBXShellScriptBuildPhase.new(project, uuid)
        run_script_phase.initialize_defaults
        run_script_phase.name = shell_name
        run_script_phase.shell_script = shell_script

        target.build_phases.insert(top ? 0 : target.build_phases.length, run_script_phase)

    end

    def build_setting_value_should_be_array(project, key)
        array_settings = Xcodeproj::Project::Object::XCBuildConfiguration::BuildSettingsArraySettingsByObjectVersion[project.object_version]
        return array_settings.include?(key)
    end

    def parse_gsdk_build_settings()
        settings_file_path = "#{framework_folderpath}/gsdk_build_settings.txt";
        settings = {}
        if File.exist?(settings_file_path)
            contents = File.read(settings_file_path)
            contents.split("\n").each do |line|
                idx = line.index('=')
                if idx != nil
                    key = line[0...idx].strip
                    value = line[idx+1...line.length].strip
                    settings[key] = value
                end
            end
        end
        settings
    end

    def should_update_buildsetting(target, setting_key)
        app_only_build_settings = [
            'ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES'
        ]
        puts "#{target.product_type}"
        if target.respond_to?('product_type') && target.product_type != 'com.apple.product-type.application'
            return !app_only_build_settings.include?(setting_key)
        end
        return true
    end

    def modify_buildsettings(target, project, framework_search_path)    
        configJson = File.read("#{ios_gsdk_home}/GMSDKConfig.json")
        configObj = JSON.parse(configJson)

        # 自3.0.1起，支持gsdk_build_settings.txt文件
        sdk_build_settings = parse_gsdk_build_settings()

        target.build_configurations.each do |config|
            
            configObj["BuildSettings"].each do |build_setting|
                if build_setting["targetName"] and build_setting["targetName"] != target.name
                    next
                end
                if build_setting["configuration"] and build_setting["configuration"] != "all" and build_setting["configuration"] != config.name
                    next
                end
                config.build_settings[build_setting["settingsName"]] = build_setting["settingsValue"]
            end
            
            
            config.build_settings['ENABLE_BITCODE'] = 'NO'

            # SEARCH PATH
            if (config.build_settings['FRAMEWORK_SEARCH_PATHS'] == nil)
                config.build_settings['FRAMEWORK_SEARCH_PATHS'] = '$(inherited)'
            end
            if !config.build_settings['FRAMEWORK_SEARCH_PATHS'].include? "$(inherited)"
                config.build_settings['FRAMEWORK_SEARCH_PATHS'] << ' $(inherited) '
            end
            if !config.build_settings['FRAMEWORK_SEARCH_PATHS'].include? framework_search_path
                config.build_settings['FRAMEWORK_SEARCH_PATHS'] << " #{framework_search_path} "
            end


            # LINKER FLAGS
            if (config.build_settings['OTHER_LDFLAGS'] == nil)
                config.build_settings['OTHER_LDFLAGS'] = '$(inherited)'
            end
            if !config.build_settings['OTHER_LDFLAGS'].include? "$(inherited)"
                config.build_settings['OTHER_LDFLAGS'] << ' $(inherited) '
            end
            if !config.build_settings['OTHER_LDFLAGS'].include? "-ObjC"
                config.build_settings['OTHER_LDFLAGS'] << ' -ObjC '
            end

            inherited = '$(inherited)'
            sdk_build_settings.each_pair do |k,v|
                next unless should_update_buildsetting(target, k)

                if build_setting_value_should_be_array(project, k)
                    content = config.build_settings[k] || inherited
                    content = content.join(' ') if content.is_a?(Array)
        
                    idx = v.index(inherited)
                    if idx != nil
                        v[idx, inherited.length] = content
                        config.build_settings[k] = v
                    else
                        content << ' ' << v
                        config.build_settings[k] = content
                    end
                else
                    config.build_settings[k] = v
                end
            end

        end
    end

    def remove_group(project, group)
        #移除framework，bundle
        if group
            group.files.each do |file|
                file_ref = group.find_file_by_path(file.path)
                if file_ref
                    if file.name.end_with?('.framework')
                        puts "移除已有的framework:#{file.name}"
                        group.remove_reference(file_ref)
                        project.targets.each do |target|
                            target.frameworks_build_phases.remove_file_reference(file_ref)
                            # 如果framework是动态库，且添加到了 Embed Frameworks 中，还需要把它从 Embed Frameworks 中移除：
                            embed_frameworks_group = nil
                            target.copy_files_build_phases.each do |e|
                                if e.display_name.end_with?("Embed Frameworks")
                                    embed_frameworks_group = e
                                    break
                                end
                            end
                            # 如果找到 Embed Frameworks ，移除索引
                            if embed_frameworks_group
                                embed_frameworks_group.remove_file_reference(file_ref)
                            end
                        end
                        
                    elsif file.name.end_with?('.bundle')
                        puts "移除已有的bundle:#{file.name}"
                        group.remove_reference(file_ref)
                        project.targets.each do |target|
                            target.resources_build_phase.remove_file_reference(file_ref)

                        end

                    end
                end
            end
        end

    end

    def add_custom_framework_entry(main_target, dynamic_target, project, framework_path, group)
        isDynamic = is_dynamic_framework(framework_path)
        if isDynamic
            puts "添加动态库: #{framework_path}"
            add_custom_framework(main_target, project, framework_path, group, true)
            if dynamic_target
                add_custom_framework(dynamic_target, project, framework_path, group, false)
            end
        else
            puts "添加静态库: #{framework_path}"
            if dynamic_target
                add_custom_framework(dynamic_target, project, framework_path, group, false)
            else
                add_custom_framework(main_target, project, framework_path, group, false)
            end
        end
    end

    def expose_public_headers(target, project, headers)
        
        headers_build_phase = nil
        target.build_phases.each do |phase|
            if phase.class.name.include?("PBXHeadersBuildPhase")
                headers_build_phase = phase
                break
            end
        end
        if !headers_build_phase
            headers_build_phase = project.new(Xcodeproj::Project::Object::PBXHeadersBuildPhase)
            target.build_phases.insert(0, headers_build_phase)
        end
        
        headers.each do |header|
            puts "暴露头文件 #{header}"
            project_file = project.files.detect { |ref| ref.path.to_s.end_with?(header)}
            
            build_file = project.new(Xcodeproj::Project::Object::PBXBuildFile)
            build_file.file_ref = project_file
            build_file.settings = { 'ATTRIBUTES' => ['Public'] }
            
            headers_build_phase.files << build_file
        end
    end
end

GMConfig.new().integrate()
