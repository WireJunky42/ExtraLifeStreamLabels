@echo on
cls

start  %~dp0\ExtraLifeStreamLabelsService.exe i

pause

sc Start ExtraLifeStreamLabelService

pause
