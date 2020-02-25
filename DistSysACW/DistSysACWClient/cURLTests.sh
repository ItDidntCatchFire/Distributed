host=""
local=0
if [ $# -eq 0 ]
 then
	host="http://distsysacw.azurewebsites.net/8285836/Api/"
		printf "Clearing\n"
	if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}'other/clear' ) == 200 ]] 
	 then
		printf "Cleared"
	 else
		printf "Can't clear"
		exit
	fi
	
  else
	host="https://localhost:5001/api/"
	local=1

	cd ../Data
	dotnet build
	dotnet ef database drop -f 
	dotnet ef database update
	cd ../DistSysACW
	dotnet run --no-build > /dev/null & 
	PROC_ID=$!
	cd ../DistSysACWClient
fi

clear
set -e
trap error SIGHUP

function error()
{
	if [[ $local == 1 ]] 
	 then
		kill $PROC_ID
	 fi
	
	printf "ERROR\n"
	printf "temp.txt\n"
	cat temp.txt
	exit 1
}

printf "Task 1 \n"
printf "\Hello World\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}talkback/hello) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "Hello World" != $var ]] && [[ "hello world" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tSort working\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?integers=8&integers=2&integers=5' ) == 200 ]]
then 
    var=$(<temp.txt)
    if [ "[2,5,8]" != $var ]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tSort empty\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?' ) == 200 ]] 
then 
    var=$(<temp.txt)
    if [ "[]" != $var ]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tSort NaN\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?integers=8&integers=2&integers=a' ) == 400 ]]
then 
    var=$(<temp.txt)
    if [[ $var != "Bad Request" ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "Task 4 \n"
printf "\tNo username Passed\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}user/new?username=) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"" != "$var" ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tUser that does not Exist\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}user/new?username=UserOne) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tAdding 'UserOne'\n"
if [[ $(curl -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '"UserOne"') == 200 ]]
then 
	var=$(<temp.txt)
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tUser that does Exist\n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}user/new?username=UserOne) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "\"True - User Does Exist! Did you mean to do a POST to create a new user?\"" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tAdding Existing username 'UserOne'\n"
if [[ $(curl -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '"UserOne"') == 403 ]]
then 
	var=$(<temp.txt)
	if [[ "\"Oops. This username is already in use. Please try again with a new username.\"" != $var ]] && [[ "Oops. This username is already in use. Please try again with a new username." != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "\tAdding no username\n"
if [[ $(curl -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '') == 400 ]]
then 
	var=$(<temp.txt)
	if [[ "Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json" != $var ]] && [[ "\"Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json\"" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;



if [[ $local == 1 ]] 
then
	kill $PROC_ID
else
	./$(basename $0) 1 && exit
fi