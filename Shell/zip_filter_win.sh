#!/bin/bash 

cd `dirname $0`

export zip=`pwd`"/7za.exe"

echo ${zip}


# 拷出bundle的资源
sh bundle_post.sh


# ${zip} a update.zip update

# 生成update目录
sh fetch_bytes.sh
cd ..
PROJ_PATH=`pwd`

function todo_zip_dir(){	
	cd ${1}
	# 删除所有.meta
	# find . -name "*.meta" | xargs rm -r
	find .  -name "*.meta" -print0 | xargs  -0 rm -f 
	cd .. 
	# compress 

	if [[ ${1} == *"/"* ]]; then
		dir=${1%/*}
		file=${1#*/}
		${zip} a ${file}.zip ${file}
		rm -rf ${file}
	else
		${zip} a ${1}.zip ${1}
		rm -rf ${1}
	fi
}

function todo_zip_table(){
	# 根据lua生成的代码自动生成copy对应的表格bytes

	FOLDER=${1}/Assets/StreamingAssets/lua/table
	files=$(ls $FOLDER)
	cd ${1}/Assets/StreamingAssets
	rm -rf table
	mkdir table
	cd ..
	for file in $files
	do
		# 将file的后缀.lua.txt去掉
		name=${file//.lua.txt/} 

		if [[ $name == *.meta ]]; then
			continue
		fi

		if [ ${name}x != "table"x ]; then
		# copy bundle res目录下生成的bytes到streamingassets目录
		cp BundleRes/Table/${name}.bytes StreamingAssets/table/
		fi
	done

	cd StreamingAssets
	${zip} a table.zip table
	rm -rf table/
}

todo_zip_table ${PROJ_PATH}


for line in `cat ${PROJ_PATH}/Shell/zip.txt`
do
	cd ${PROJ_PATH}/Assets/StreamingAssets/
	todo_zip_dir ${line}
done

cp ${PROJ_PATH}/Shell/zip.txt ${PROJ_PATH}/Assets/StreamingAssets/zipinfo.txt
