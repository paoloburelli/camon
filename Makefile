all: package

test:
	mkdir -p build; open /Applications/Unity/Unity.app --args -projectPath ${PWD} -buildOSXPlayer ${PWD}/build/CameramanTest.app -batchMode -quit

package:
	mkdir -p build; open /Applications/Unity/Unity.app --args -projectPath ${PWD} -exportPackage Assets/Cameraman ${PWD}/build/Cameraman.unitypackage -batchMode -quit

clean:
	$(RM) -r build
