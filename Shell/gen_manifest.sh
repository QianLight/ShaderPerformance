# ===================================================================
# 生成热更用的资源manifest
# --------------------------------------------------------------------
# Auth:   Huailiang.Peng
# Data:   2021.01.29
# ====================================================================
#!/bin/bash 

if [ $# != 1 ]; then
	echo "请先输入update文件夹"
	exit 1
fi

echo "Bundle目录："${1}

cd ${1}

IFS=$'\n' # 这个必须要，否则会在文件名中有空格时出错
find .  -name "*.meta" -print0 | xargs  -0 rm -f 

OUTPUT=manifest.txt
CONTEXT=""

function ergodic(){
  for file in `ls $1`
  do
    if [ -d $1"/"$file ]
    then
      ergodic $1"/"$file
    else
      local path=${1}"/"$file 
      # echo ${path:2}
      CONTEXT=${CONTEXT}${path:2}"\n"
    fi
  done
}

if [[ -f ${OUTPUT} ]]; then
	rm -f ${OUTPUT}
fi


INIT_PATH=".";
ergodic $INIT_PATH

# windows 加一个-e, mac上直接输出
# echo -e ${CONTEXT}>${OUTPUT}
echo ${CONTEXT}>${OUTPUT}

echo "manifest.txt已经生成, Bye"