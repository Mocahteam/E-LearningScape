#!/bin/bash

UNITY_VERSION=$(grep "m_EditorVersion:" ./$UNITY_PROJECT_FOLDER/ProjectSettings/ProjectVersion.txt | awk '{print $2}')

/Applications/Unity/Hub/Editor/$UNITY_VERSION/Unity.app/Contents/MacOS/Unity \
	-batchmode \
	-silent-crashes \
	-projectPath $(PWD)/$UNITY_PROJECT_FOLDER \
	-logFile - \
	-buildTarget $BUILD_TARGET \
	-buildPath $BUILD_FOLDER/$BUILD_NAME \
	-executeMethod Builds.BuildFromCLI \
	-bundleVersion $BUNDLE_VERSION \
	-quit

BUILD_SUCCESS=$?

RED="\033[0;31m"
GREEN="\033[0;32m"
NOCOLOR="\033[0m"

if [ $BUILD_SUCCESS -eq 0 ]
then
	echo -e "${GREEN}Build successful${NOCOLOR}"
else
	echo -e "${RED}Build has failed${NOCOLOR}"
fi

exit $BUILD_SUCCESS
