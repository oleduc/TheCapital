#!/bin/bash
cd ${0%/*}
RW_PATH="/d/Steam/steamapps/common/RimWorld"
cp --verbose -rf ../TheCapital/bin/Debug/0Harmony.dll ../TheCapital/Mod/Assemblies/
cp --verbose -rf ../TheCapital/bin/Debug/TheCapital.dll ../TheCapital/Mod/Assemblies/
rm -r "$RW_PATH/Mods/TheCapital"
mkdir "$RW_PATH/Mods/TheCapital"
cp --verbose -rf ../TheCapital/Mod/* "$RW_PATH/Mods/TheCapital"
