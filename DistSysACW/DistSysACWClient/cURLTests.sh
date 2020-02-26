host=""
local=0


APIKey=""

if [ $# -eq 0 ]
 then
	host="http://distsysacw.azurewebsites.net/8285836/Api/"
		printf "Clearing\n"
	if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}'other/clear' ) == 200 ]] 
	 then
		printf "Cleared"
	 else
		printf "Can't clear"
		exit
	fi
	
  else
	host="https://localhost:5001/api/"
	local=1

	cd ..
	dotnet build
	cd Data
	export PATH="$PATH:$HOME/.dotnet/tools/" 
	dotnet ef database drop -f 
	dotnet ef database update
	cd ../DistSysACW
	dotnet run --no-build > /dev/null &
	PROC_ID=$!
	sleep 2
	printf "process ID: "$PROC_ID"\n"
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
printf "\tHello World\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}talkback/hello) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "Hello World" != $var ]] && [[ "hello world" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tSort working\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?integers=8&integers=2&integers=5' ) == 200 ]]
then 
    var=$(<temp.txt)
    if [ "[2,5,8]" != $var ]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tSort empty\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?' ) == 200 ]] 
then 
    var=$(<temp.txt)
    if [ "[]" != $var ]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tSort NaN\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?integers=8&integers=2&integers=a' ) == 400 ]]
then 
    var=$(<temp.txt)
    if [[ $var != "Bad Request" ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "Task 4 \n"
printf "\tNo username Passed\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}user/new?username=) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"" != "$var" ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tUser that does not Exist\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}user/new?username=UserOne) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "\"False - User Does Not Exist! Did you mean to do a POST to create a new user?\"" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tAdding 'UserOne'\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '"UserOne"') == 200 ]]
then 
	var=$(<temp.txt)
	APIKey=$var
	printf "\t\t APIKEY: $APIKey\n" 
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tUser that does Exist\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}user/new?username=UserOne) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "\"True - User Does Exist! Did you mean to do a POST to create a new user?\"" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tAdding Existing username 'UserOne'\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '"UserOne"') == 403 ]]
then 
	var=$(<temp.txt)
	if [[ "\"Oops. This username is already in use. Please try again with a new username.\"" != $var ]] && [[ "Oops. This username is already in use. Please try again with a new username." != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tAdding no username\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '') == 400 ]]
then 
	var=$(<temp.txt)
	if [[ "Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json" != $var ]] && [[ "\"Oops. Make sure your body contains a string with your username and your Content-Type is Content-Type:application/json\"" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\e[31m\tAdding [SPACE] as username\n\e[m"
# if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d ' ') == 200 ]]
# then 
# 	var=$(<temp.txt)
#     printf "\t\t APIKEY: $var\n" 
# else
#     printf "  http code Fail\n"
# 	kill -1 $$
# fi;

printf "Task 7 \n"
printf "\tDeleting a user unauthorized\n"
if [[ $(curl -s -k -o temp.txt -X DELETE -w '%{http_code}' ${host}user/RemoveUser?username=UserOne -H 'ApiKey: '$APIKey'h') == 401 ]]
then 
	var=$(<temp.txt)
	if [[ "\"Unauthorized. Check ApiKey in Header is correct."\" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tDeleting a non-existant user (UserZero)\n"
if [[ $(curl -s -k -o temp.txt -X DELETE -w '%{http_code}' ${host}user/RemoveUser?username=UserZero -H 'ApiKey: '$APIKey) == 200 ]]
then 
	var=$(<temp.txt)
	if [[ "false" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;


printf "\tDeleting a user Authorized (User)\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '"UserTwo"') != 200 ]]
then
    printf "\tUserTwo failed\n"
    kill -1 $$
else
    var=$(<temp.txt)	 
    printf "\t\t APIKEY: $var\n"
    if [[ $(curl -s -k -o temp.txt -X DELETE -w '%{http_code}' ${host}user/RemoveUser?username=UserTwo -H 'ApiKey: '$var) == 200 ]]
    then 
    	var=$(<temp.txt)
    	if [[ "true" != $var ]]
    	then
            printf "Failed \n"
    		kill -1 $$
        fi;  
    else
        printf "  http code Fail\n"
    	kill -1 $$
    fi;
fi;

printf "Task 8 \n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/new -H 'Content-Type: application/json' -d '"UserTwo"') != 200 ]]
then
    printf "\tUserTwo failed\n"
    kill -1 $$
fi;

printf "\tChanging UserTwo to Admin\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/ChangeRole -H 'ApiKey: '$APIKey -H 'Content-Type: application/json' -d '{"username": "UserTwo","role": "Admin"}') == 200 ]]
then 
	var=$(<temp.txt)
	if [[ "DONE" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tChanging UserTwo to User\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/ChangeRole -H 'ApiKey: '$APIKey -H 'Content-Type: application/json' -d '{"username": "UserTwo","role": "User"}') == 200 ]]
then 
	var=$(<temp.txt)
	if [[ "DONE" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tChanging non-existant (UserZero) to User\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/ChangeRole -H 'ApiKey: '$APIKey -H 'Content-Type: application/json' -d '{"username": "UserZero","role": "User"}') == 400 ]]
then 
	var=$(<temp.txt)
	if [[ "NOT DONE: Username does not exist" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tChanging non-existant (UserZero) to Admin\n"
if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/ChangeRole -H 'ApiKey: '$APIKey -H 'Content-Type: application/json' -d '{"username": "UserZero","role": "Admin"}') == 400 ]]
then 
	var=$(<temp.txt)
	if [[ "NOT DONE: Username does not exist" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\e[31m\tChanging role non JSON\n\e[m"
# if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/ChangeRole -H 'ApiKey: '$APIKey -H 'Content-Type: application/json' -d 'HELLO') == 400 ]]
# then 
# 	var=$(<temp.txt)
# 	if [[ "NOT DONE: An error occured" != $var ]]
# 	then
#         printf "Failed \n"
# 		kill -1 $$
#     fi;  
# else
#     printf "  http code Fail\n"
# 	kill -1 $$
# fi;

printf "\e[31m\tChanging role to King\n\e[m"
# if [[ $(curl -s -k -o temp.txt -X POST -w '%{http_code}' ${host}user/ChangeRole -H 'ApiKey: '$APIKey -H 'Content-Type: application/json' -d '{"username": "UserTwo","role": "King"}') == 400 ]]
# then 
# 	var=$(<temp.txt)
# 	if [[ "NOT DONE: Role does not exist" != $var ]]
# 	then
#         printf "Failed \n"
# 		kill -1 $$
#     fi;  
# else
#     printf "  http code Fail\n"
# 	kill -1 $$
# fi;


printf "Task 9 \n"
printf "\tProtected Hello (Admin)\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}protected/hello -H 'ApiKey: '$APIKey) == 200 ]]
then 
	var=$(<temp.txt)
	if [[ "Hello UserOne" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\e[31m\tProtected Hello (User)\n\e[m"
# if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}protected/hello -H 'ApiKey: '$APIKey) == 401 ]]
# then 
# 	var=$(<temp.txt)
#     if [[ "\"Unauthorized. Check ApiKey in Header is correct."\" != $var ]]
#     then
#         printf "Failed \n"
#         kill -1 $$
#     fi;    
# else
#     printf "  http code Fail\n"
# 	kill -1 $$
# fi;

printf "\tProtected Hello unauthorized\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}protected/hello -H 'ApiKey: '$APIKey'h') == 401 ]]
then 
	var=$(<temp.txt)
    if [[ "\"Unauthorized. Check ApiKey in Header is correct."\" != $var ]]
    then
        printf "Failed \n"
        kill -1 $$
    fi;    
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\tProtected sha1 hello (Admin)\n"
if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}protected/sha1?message=hello -H 'ApiKey: '$APIKey) == 200 ]]
then 
	var=$(<temp.txt)
	if [[ "AAF4C61DDCC5E8A2DABEDE0F3B482CD9AEA9434D" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail\n"
	kill -1 $$
fi;

printf "\e[31m\tProtected sha1 hello (User)\n\e[m"
# if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}protected/sha1?message=hello -H 'ApiKey: '$APIKey) == 200 ]]
# then 
# 	var=$(<temp.txt)
# 	if [[ "AAF4C61DDCC5E8A2DABEDE0F3B482CD9AEA9434D" != $var ]]
# 	then
#         printf "Failed \n"
# 		kill -1 $$
#     fi;  
# else
#     printf "  http code Fail\n"
# 	kill -1 $$
# fi;


printf "\e[31m\tProtected sha1 hello unauthorized\n\e[m"
# if [[ $(curl -s -k -o temp.txt -w '%{http_code}' ${host}protected/sha1?message=hello -H 'ApiKey: '$APIKey'h') == 401 ]]
# then 
# 	var=$(<temp.txt)
#     if [[ "\"Unauthorized. Check ApiKey in Header is correct."\" != $var ]]
#     then
#         printf "Failed \n"
#         kill -1 $$
#     fi;    
# else
#     printf "  http code Fail\n"
# 	kill -1 $$
# fi;


if [[ $local == 1 ]] 
then
	kill $PROC_ID
else
	./$(basename $0) 1 && exit
fi

#$(curl -k -i -X OPTIONS http://distsysacw.azurewebsites.net/8285836/Api/protected/hello -H 'ApiKey: 1414aab0-fb56-4371-9686-6bd74238524d')