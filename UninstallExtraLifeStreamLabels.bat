@echo on
cls

sc Stop ExtraLifeStreamLabelService

pause

start %~dp0\ExtraLifeStreamLabelsService.exe u

pause
