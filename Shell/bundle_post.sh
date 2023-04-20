# ===================================================================
# 由于Android包超过2G， 特把一些资源提取出来， 并生成manifest
# 此脚本在打完bundle， 生成apk之前执行
# --------------------------------------------------------------------
# Auth:   Huailiang.Peng
# Data:   2021.01.29
# ====================================================================
#!/bin/bash 

cd `dirname $0`

cd ..

PROJ_PATH=`pwd`

cd Shell

rm -rf update

rm -rf update.zip

function todo_copy_ext(){
	mkdir -p update/${1}
	mv ${PROJ_PATH}/Assets/StreamingAssets/Bundles/assets/bundleres/${1} update/
}

# todo_copy_ext animation

# todo_copy_ext assetres


for line in `cat ${PROJ_PATH}/Assets/Editor/Patch/external.txt`
do
	todo_copy_ext ${line}
done


# mkdir -p update/animation

# mv ${PROJ_PATH}/Assets/StreamingAssets/Bundles/assets/bundleres/animation update/

# mkdir -p update/assetres

# mv ${PROJ_PATH}/Assets/StreamingAssets/Bundles/assets/bundleres/assetres update/

# 生成manifest.txt
# sh gen_manifest.sh ${PROJ_PATH}/Shell/update

# 压缩成zip
zip -qr update.zip update
rm -rf patch.zip
cp update.zip patch.zip