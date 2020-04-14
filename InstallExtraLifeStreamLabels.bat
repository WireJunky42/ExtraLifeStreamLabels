@echo on
cls

start  %~dp0\ExtraLifeStreamLabelsService.exe i

pause

sc Start ExtraLifeStreamLabelService

echo Press any key to exit
pause