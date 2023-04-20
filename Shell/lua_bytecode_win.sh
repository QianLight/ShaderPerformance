# ===================================================================
# 用于将streamingAssets目录下的lua转换为bytecode
# --------------------------------------------------------------------
# Auth:   Huailiang.Peng
# Data:   2020.04.26
# ====================================================================
#!/bin/bash 


cd `dirname $0`

SHELL_PATH=`pwd`

LUA53=${SHELL_PATH}"/lua53"

cd ../Assets/StreamingAssets/lua

LUAPROJ=`pwd`

cd ${LUAPROJ}

files=$(ls $FOLDER)


function make_byte_dir()
{
	cd ${LUAPROJ}/..
	mkdir -p lua2

	for element in `ls $1`
    do  
        dir_or_file=$1"/"$element
        if [ -d $dir_or_file ]
        then 
        	tmp=${dir_or_file/lua\//lua2\/}
			# echo $tmp
			mkdir -p $tmp
			make_byte_dir ${dir_or_file}
        fi  
    done
}


# main.lua 是由c# load的 这里忽略
# protoc.lua 是 protobuf 插件使用的，这里不处理
function gen_bytecode()
{
	tmp=${1/${LUAPROJ}/''}
	target=${LUAPROJ/StreamingAssets\/lua/StreamingAssets\/lua2}
	echo ${1}
	if  [[ $dir_or_file == *main.lua.txt ]]; then
		echo "ignore main.lua"
		cp ${1} ${target}${tmp}
	elif [[ ${1} == *protoc.lua.txt ]]; then
		echo "ignore protoc.lua"
		cp ${1} ${target}${tmp}
	else
		$LUA53/luac.exe -o ${target}${tmp} ${1}
	fi
}



function lua_gen_dir()
{
    for element in `ls $1`
    do  
        dir_or_file=$1"/"$element
        if [ -d $dir_or_file ]
        then 
            lua_gen_dir $dir_or_file
        else
        	if [[ $dir_or_file == *.lua.txt ]]; then
            	gen_bytecode $dir_or_file
        	fi
        fi  
    done
}


# 先创建文件结构目录 
make_byte_dir ${LUAPROJ}
# 生成bytecode
lua_gen_dir ${LUAPROJ}

