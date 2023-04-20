AAB_PATH=
APKS_PATH=
rm -f $APKS_PATH
java -jar "./bundletool-all.jar" build-apks --bundle=$AAB_PATH --output=$APKS_PATH