
#provisioning GUID 
PROVISIONINGGUID=`security cms -D -i $1 | tr "\n" " " | grep "<key>Name</key>.*<string>.*</string>" -o | awk -F\> '{print $4}' | awk -F\< '{print $1}'`
echo $PROVISIONINGGUID

#TeamID
TEAMIDETIFIER=`security cms -D -i $1 | tr "\n" " " | grep "<key>com.apple.developer.team-identifier</key>.*<string>.*</string>" -o | awk -F\> '{print $4}' | awk -F\< '{print $1}'`
echo $TEAMIDETIFIER

#CERT
CERTCONTENT=`/usr/libexec/PlistBuddy -c 'Print DeveloperCertificates:0' /dev/stdin <<< $(security cms -D -u 11 -i "$1") | openssl x509 -subject -inform der | head -n 1 | grep -o "iPhone .*/OU"`
if [ $CERTCONTENT=="" ];
then
    CERTCONTENT=`/usr/libexec/PlistBuddy -c 'Print DeveloperCertificates:0' /dev/stdin <<< $(security cms -D -u 11 -i "$1") | openssl x509 -subject -inform der | head -n 1 | grep -o "Apple .*/OU"`
fi
echo ${CERTCONTENT%/OU}