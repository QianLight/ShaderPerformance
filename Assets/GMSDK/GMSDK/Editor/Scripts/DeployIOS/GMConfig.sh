#!/bin/bash

ProjPath=$1
TargetName=$2
GSDKPath=$3
dynamicName=$4

if [[ ! -e $GSDKPath/gsdk ]] && [[ -e $GSDKPath/bdgmProduct ]] ; then
    ln -s bdgmProduct $(cd "$GSDKPath"; pwd)/gsdk
fi

MainDynamic="$GSDKPath/gsdk/BD_GameSDK_iOS.framework/BD_GameSDK_iOS"

if [ -f "${MainDynamic}" ];
then
    `chmod u+x,g+x,o+x "${MainDynamic}"`
else
    echo "${MainDynamic} not found"
    exit
fi

path=`dirname $0`
cd $(dirname $(dirname $ProjPath))

if [ -f ./ruby.zip ];
then
    unzip -q -o ruby.zip -d ./
else
    echo "Ruby.zip not found"
    exit
fi

absolutePath=`pwd`

export PATH="${absolutePath}/ruby/bin:$PATH"

ruby_exe="ruby -I./ruby/lib/ruby/2.5.0 -I./ruby/lib/ruby/2.5.0/x86_64-darwin19"
cmd="$ruby_exe GMConfig.rb -project_path $(dirname $ProjPath) -project_target_name $TargetName -framework_folderpath $GSDKPath"

if [ "$dynamicName" != "" ]; then
    cmd="$cmd -dynamic_target_name $dynamicName"
fi

echo "run cmd: $cmd"

$cmd 2>&1

rm -rf ./ruby.zip
rm -rf ./ruby
rm ./GMConfig.rb
