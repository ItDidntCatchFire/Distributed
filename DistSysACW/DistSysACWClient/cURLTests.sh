host="https://localhost:5001/api/"

printf "Task 1 \n"
if [[ $(curl -k -o temp.txt -w '%{http_code}' ${host}talkback/hello) == 200 ]]
then 
    var=$(<temp.txt)
    if [[ "Hello World" != $var ]]
    then
        printf "Failed \n"  
    fi;  
else
    printf "  http code Fail"
fi;
