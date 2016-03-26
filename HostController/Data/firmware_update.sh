cd /home/pi/onbs/
git pull

sudo xbuild /p:Configuration=Release OnboardSytem.sln

cd /home/pi/onbs/_bin/Application
sudo mono HostController.exe