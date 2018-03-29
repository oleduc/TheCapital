#!/bin/bash
RW_PATH="/d/Steam/steamapps/common/RimWorld"
cp -rf ../TheCapital/bin/Debug/TheCapital.dll ../TheCapital/Mod/Assemblies/
cp -rf ../TheCapital/Mod/* "$RW_PATH/Mods/TheCapital"
