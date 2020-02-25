for i in {1..10000}
do

kill -9 $(ps aux | grep 'bash' | grep -v 'grep' | awk '{print $2}') &

done