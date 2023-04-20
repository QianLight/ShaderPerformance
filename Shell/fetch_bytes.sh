# ===================================================================
# 这里主要是根据zip.txt这个配置 生成update目录
# update是android上第一次运行， 拷贝到persist目录，会被热更覆盖
# --------------------------------------------------------------------
# Auth:   Huailiang.Peng
# Data:   2021.01.25
# ====================================================================
#!/bin/bash 

cd `dirname $0`
cd ..
PROJ_PATH=`pwd`

DEST=${PROJ_PATH}/Assets/StreamingAssets/

function todo_copy_dir(){	
	echo "${1}"
	cd ${DEST}
	if [[ ${1} == *"update/"* ]]; then
		file=${1#*/}
		cd update
		mkdir ${file}
		cd ${DEST}"Bundles/assets/bundleres/"${file}
		# find ${DEST}"Bundles/assets/bundleres/"${file} -name "*hit01.bytes" | xargs -I % cp -R % ${DEST}"update/"${file}
		pwd
		arr=`find . -name "*.bytes"`
		for it in ${arr}
		do
			it2=${it:2}  # 去除前面的./
			if [[ -f ${it2} ]]; then
				it3=${it2%/*}  # 去除最后的文件名，拿到目录； 如果原样输出， 则表示不存在子目录
				cd ${DEST}"update/"${file}
				if [[ ${it2}x == ${it3}x ]]; then
					cd ${DEST}"Bundles/assets/bundleres/"${file}
					# cp -pR ${it2} ${DEST}"update/"${file}
					mv -f ${it2} ${DEST}"update/"${file}
				else
					if [ ! -d ${it3}  ];then
		  				mkdir -p ${it3}
					fi
					cd ${DEST}"Bundles/assets/bundleres/"${file}
					# cp -pR ${it2} ${DEST}"update/"${file}"/"${it3}
					mv -f ${it2} ${DEST}"update/"${file}"/"${it3}
				fi
			fi
		done 
	fi
}

cd ${DEST}

rm -rf update

mkdir update



for line in `cat ${PROJ_PATH}/Shell/zip.txt`
do
	todo_copy_dir ${line}
done
