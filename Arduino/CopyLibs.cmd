rd /S /Q "%userprofile%\Documents\Arduino\libraries"
xcopy "%CD%\libraries\*.*" "%userprofile%\Documents\Arduino\libraries\" /E /Y
xcopy "%CD%\AMC_Hardware_Controller\*.*" "%userprofile%\Documents\Arduino\AMC_Hardware_Controller\" /E /Y