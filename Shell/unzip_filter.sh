# ===================================================================
# 针对 特定资源 文件夹进行压缩操作 
# 1. 根据zip.txt将对应的文件夹zip压缩
# 2. 将bundleres/table里的lua用到的表格copy到streamingassets目录 并zip打包
# --------------------------------------------------------------------
# Auth:   Huailiang.Peng
# Data:   2020.04.09
# ====================================================================
#!/bin/bash 

cd `dirname $0`
cd ..
PROJ_PATH=`pwd`

echo $0
echo $PROJ_PATH


function todo_unzip_dir(){	
	echo "${1}"

	if [[ ${1} == *"/"* ]]; then
		# 包含子文件夹
		dir=${1%/*}
		cd ${dir}
		file=${1#*/}
		unzip ${file}.zip
		rm -f ${file}.zip
	else
		pwd
		echo "direct unzip "${1}
		unzip ${1}.zip 
		rm -f ${1}.zip
	fi
}


for line in `cat ${PROJ_PATH}/Shell/zip.txt`
do
	cd ${PROJ_PATH}/Assets/StreamingAssets/
	todo_unzip_dir ${line}
done



