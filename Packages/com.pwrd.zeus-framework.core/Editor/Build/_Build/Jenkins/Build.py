# -*- coding: utf-8 -*-
import subprocess
import sys, os
import shutil
import time
target = os.getenv("ZEUS_PLATFORM", "Android").lower()
if target == "android":
    target = "Android"
elif target == "windows":
    target = "Windows"
elif target == "ios":
    target = "iOS"
from PIL import Image
from MyQR import myqr

def showTime(t):
    m, s = divmod(t, 60)
    h, m = divmod(m, 60)
    return ("%02dhours%02dminutes%02seconds" % (h, m, s))

startTime = time.time()

unityPath = os.getenv("UNITY_EXE_PATH")
workspace = os.getenv("WORKSPACE")
workspaceURL = os.getenv("JOB_URL") + "ws"
loginInfo = " -username " + os.getenv("UnityAccount") + " -password " + os.getenv("UnityPassword")

dirName = target + "_" + os.getenv("ZEUS_HOTFIX_CHANNEL") + "_" + time.strftime("%m%d%H%M") + "_" + os.getenv("ZEUS_BUNDLE_VERSION") + "." + os.getenv("ZEUS_BUNDLE_NUMBER")
outputDir = "/Output/" + dirName
outputPath = workspace + outputDir

if os.getenv("BUILD_ASSETBUNDLE")  == "true":
    #AssetBundle打包
    resProjectPath = os.getenv("RES_PATH")
    keyValuePairList = "-target " + target
    keyValuePairList += " -graph Assets/AssetGraph.asset"
    logPath = workspace + outputDir + "/Logs/BuildAB.log"

    print("begin build AssetBundle")
    subprocess.call(unityPath + loginInfo + " -accept -apiupdate -quit -batchmode -projectPath " + resProjectPath + " -logFile " + logPath + " -executeMethod UnityEngine.AssetGraph.CUIUtility.BuildFromCommandline " + keyValuePairList, shell = True)
    print("finish build AssetBundle")
    endTime = time.time()
    print("AssetBundle building time cost: %s" %showTime(endTime - startTime))
    print(workspaceURL + outputDir + "/Logs/BuildAB.log")
endTime = time.time()

#Unity打包
projectPath = os.getenv("PROJECT_PATH")
keyValuePairList = ""
env_dist = os.environ

for key in env_dist:
	if key.find("ZEUS_") != -1:
		keyValuePairList += (" " + key.replace("ZEUS_", "") + "=" + env_dist[key])

#输出路径在workspace下
#XcodeProject单独存放一个路径
if target == "iOS":
    XcodeProjPath = workspace + "/Output/XcodeProject"
    keyValuePairList += (" OUTPUT_PATH=" + XcodeProjPath)
else:
    keyValuePairList += (" OUTPUT_PATH=" + outputPath)
logPath = workspace + outputDir + "/Logs/build.log"
print("begin build Unity")

subprocess.call(unityPath + loginInfo + " -accept -apiupdate -quit -batchmode -projectPath " + projectPath + " -logFile " + logPath + " -buildTarget " + target.lower() + " -executeMethod Zeus.Build.BuildScript.BuildPlayerInBatchMode " + keyValuePairList, shell = True)

print("finish build Unity")
print("Unity building time cost: %s" %showTime(time.time() - endTime))
print(workspaceURL + outputDir + "/Logs/build.log")

if target == "iOS":
    #Xcode打包
    XcodeProjFile = XcodeProjPath + "/" + os.getenv("ZEUS_PACKAGE_NAME") + "/Unity-iPhone.xcodeproj"
    #Clean Xcode
    print("begin clean xcode")
    clearLogPath = workspace + outputDir + "/Logs/XcodeClean.log"
    fileObj = open(clearLogPath, 'w')
    result = subprocess.call('xcodebuild -UseModernBuildSystem=0 clean -project ' + XcodeProjFile, stdout = fileObj, shell = True)
    fileObj.close()
    if result == 0:
        print("Xcode clean success")
    else:
        print("Xcode clean failed")
        exit(1)
    #创建archive
    archiveLogPath = workspace + outputDir + "/Logs/XcodeArchive.log"
    fileObj = open(archiveLogPath, 'w')
    archivePath = XcodeProjPath + "/" + os.getenv("ZEUS_PACKAGE_NAME") + "/Archive/" + os.getenv("ZEUS_PACKAGE_NAME") + ".xcarchive"
    print("begin xcarchive")
    result = subprocess.call('xcodebuild -project ' + XcodeProjFile + ' -scheme "Unity-iPhone" ENABLE_BITCODE=NO -allowProvisioningUpdates -archivePath ' + archivePath + ' clean archive build', stdout = fileObj, shell = True)
    fileObj.close()
    if result == 0:
        print("Xcode to xcarchive success")
    else:
        print("Xcode to xcarchive failed")
        exit(1)
    #创建ipa
    ipaLogPath = workspace + outputDir + "/Logs/IpaBuild.log"
    plistPath = XcodeProjPath + "/" + os.getenv("ZEUS_PACKAGE_NAME") + "/Info.plist"
    fileObj = open(ipaLogPath, 'w')
    print("begin build ipa")
    result = subprocess.call('xcodebuild -exportArchive -archivePath ' + archivePath + ' -target "Unity-iPhone" ENABLE_BITCODE=NO -exportOptionsPlist ' + plistPath + ' -exportPath ' + outputPath, stdout = fileObj, shell = True)
    fileObj.close()
    if result == 0:
        print("Xcarchive to ipa success")
    else:
        print("Xcarchive to ipa failed")
        exit(1)
    print("Xcode building time cost: %s" %showTime(time.time() - endTime))
    
#打印URL路径
pkgName = os.getenv("ZEUS_PACKAGE_NAME", "Test")
appName = pkgName + ".apk"
if target == "iOS":
    appName = "Unity-iPhone.ipa"
else:
	appURL = workspaceURL + outputDir + "/" + appName
	print(appURL)
if os.getenv("UseSubpackage", "false").lower() == "true":
	if target == "iOS":
		#如果是iOS打包，需要把subpackage文件移动一下
		srcSubpackagefile = XcodeProjPath + "/" + pkgName + ".subpackage"
		destSubpackagefile = outputPath + "/" + pkgName + ".subpackage"
		shutil.move(srcfile, dstfile) 
	print(workspaceURL + outputDir + "/" + pkgName + ".subpackage")

#生成二维码
if target == "iOS":
    #这里设置IP地址
    appURL = "https://10.5.32.20/Jenkins/" + dirName + ".ipa"
    destDir = "/Library/WebServer/Documents/Jenkins"
    if not os.path.exists(destDir):
    	os.makedirs(destDir)
    shutil.copy((outputPath + "/Unity-iPhone.ipa"), (destDir + "/" + dirName + ".ipa"))
    newPlist = "/Library/WebServer/Documents/Jenkins/" + dirName + ".plist"
    with open(newPlist, 'w') as fileObj:
        fileObj.write('''<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>items</key>
    <array>
        <dict>
            <key>assets</key>
            <array>
                <dict>
                    <key>kind</key>
                    <string>software-package</string>
                    <key>url</key>
                    <string>''' + appURL + '''</string>
                </dict>
            </array>
            <key>metadata</key>
            <dict>
                <key>bundle-identifier</key>
                <string>''' + os.getenv("APPLICATION_IDENTIFIER") + '''</string>
                <key>bundle-version</key>
                <string>''' + os.getenv("ZEUS_BUNDLE_VERSION") + '''</string>
                <key>kind</key>
                <string>software</string>
                <key>title</key>
                <string>''' + os.getenv("ZEUS_PACKAGE_NAME") + '''</string>
            </dict>
        </dict>
    </array>
</dict>
</plist>''')
        plistURL = "itms-services://?action=download-manifest&url=https://10.2.52.108/Jenkins/" + dirName + ".plist"
        print(plistURL)
        appURL = plistURL
        
qrpath = outputPath + "/QRCode"
if not os.path.exists(qrpath):
        os.makedirs(qrpath)
myqr.run(words=appURL,version=9, save_name=os.getenv("JOB_NAME") + "_" + os.getenv("BUILD_NUMBER") + "_" + os.getenv("ZEUS_PACKAGE_NAME", "Test") + "_Download.png", save_dir=qrpath)
qrURL = workspaceURL + outputDir + "/QRCode/" + os.getenv("JOB_NAME") + "_" + os.getenv("BUILD_NUMBER") + "_" + os.getenv("ZEUS_PACKAGE_NAME", "Test") + "_Download.png"
print(qrURL)

#修改Jenkins环境变量
filePath = workspace + "/Temp/env.properties"
with open(filePath, 'w') as fileObj:
    fileObj.write("QRURL=" + qrURL)
    fileObj.write("\nBUILD_NAME=" + os.getenv("BUILD_NUMBER") + "_" + os.getenv("BUILD_USER") + "_" + os.getenv("ZEUS_PACKAGE_NAME"))
    fileObj.close()

print("Total time cost: %s" %showTime(time.time() - startTime))
