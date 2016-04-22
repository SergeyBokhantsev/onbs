cd /home/pi/onbs/
sudo git stash
sudo git pull

sudo rm -rf /home/pi/onbs/_bin_back
sudo mv /home/pi/onbs/_bin /home/pi/onbs/_bin_back
sudo xbuild /p:Configuration=Release OnboardSytem.sln

cd /home/pi/onbs/_bin/Application
sudo mono HostController.exe