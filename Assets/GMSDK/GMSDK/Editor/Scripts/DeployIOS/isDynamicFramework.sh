result=$(file $1 | grep "dynamically linked shared library")
if [[ "$result" != "" ]]
then
	echo "true"
else
	echo "false"
fi