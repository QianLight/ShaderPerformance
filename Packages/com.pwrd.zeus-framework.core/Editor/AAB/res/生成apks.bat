set AAB_PATH=
set APKS_PATH=
if exist %APKS_PATH% DEL %APKS_PATH%
java -jar "./bundletool-all.jar" build-apks --bundle=%AAB_PATH% --output=%APKS_PATH%