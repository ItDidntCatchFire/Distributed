host="https://localhost:5001/api/"
#host="http://distsysacw.azurewebsites.net/8285836/Api/"
clear

set -e
trap error SIGHUP 

function error()
{
	printf "ERROR\n"
	printf "temp.txt\n"
	cat temp.txt
	exit 1
}


printf "Task 1 \n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}talkback/hello) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "Hello World" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

#Sort working
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

#Sort empty
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

#Sort NaN
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}'talkback/sort?integers=8&integers=2&integers=a' ) == 400 ]]
then 
    var=$(<temp.txt)
    if [[ "$var" != *"The value 'a' is not valid."* ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

printf "Task 4 \n"

#No username Passed
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}user/new?username=) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "False - User Does Not Exist! Did you mean to do a POST to create a new user?" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

#User that does not Exist
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}user/new?username=UserOne) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "False - User Does Not Exist! Did you mean to do a POST to create a new user?" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;

#User that does Exist
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}user/new?username=UserOne) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "True - User Does Exist! Did you mean to do a POST to create a new user?" != $var ]]
	then
        printf "Failed \n"
		kill -1 $$
    fi;  
else
    printf "  http code Fail"
	kill -1 $$
fi;