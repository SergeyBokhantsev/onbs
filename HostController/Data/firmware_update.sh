cd /home/pi/onbs/
sudo git stash
sudo git pull

sudo xbuild /p:Configuration=Release OnboardSytem.sln

cd /home/pi/onbs/_bin/Application
sudo mono HostController.exe