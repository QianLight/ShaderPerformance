#!/bin/bash


srcPath=$(pwd)/../Assets/Scripts/lua
destPath=$(pwd)/../Assets/StreamingAssets/lua
luaPath=$(pwd)/lua53/luac.exe

if [ -d $destPath ] 
then
        rm -rf $destPath
fi
mkdir $destPath

function genluabytes(){
    for file in ` ls $1 `
    do
        if [ -d $1"/"$file ]
        then
                        srcDir=$1"/"$file
                        descDir=${srcDir/Scripts/StreamingAssets}
                        mkdir $descDir
                        genluabytes $1"/"$file
                 else
            filename=$1"/"$file 
            if [[ ! $filename =~ \.meta$ ]];then
                                srcFile=$1"/"$file
                                destFile=${srcFile/Scripts/StreamingAssets}
                                #echo $srcFile
                                #echo $destFile
                                $luaPath -o $destFile $srcFile
            fi        
        fi
    done
}

genluabytes $srcPath