#include<dinput.h>
#include<dinputd.h>
#include<vector>
#include<tchar.h>


struct DEVICE {
	bool hasForceFeedback = false;
	TCHAR productName[MAX_PATH];
	TCHAR effectNames[30][MAX_PATH];
	int num_effects = 0;
	LPDIRECTINPUTDEVICE8 deviceInterface;
	LPDIRECTINPUTEFFECT springForce;
	LPDIRECTINPUTEFFECT damperForce;
	LPDIRECTINPUTEFFECT constantForce;
};

LPDIRECTINPUT8 directInput = NULL;
std::vector<DEVICE>  allDevices;

BOOL CALLBACK EnumDevicesCallback(const LPCDIDEVICEINSTANCE lpDeviceInstance, LPVOID pvRef)
{
	HRESULT hr;

	// get interface for this device
	LPDIRECTINPUTDEVICE8 currentDevice;
	hr = directInput->CreateDevice(lpDeviceInstance->guidInstance, &currentDevice, NULL);

	if (FAILED(hr)) {
		return DIENUM_CONTINUE;
	}

	// store some info about this device
	DEVICE device;
	ZeroMemory(&device, sizeof(device));

	device.deviceInterface = currentDevice;
	_tcscpy_s(device.productName, _countof(device.productName), lpDeviceInstance->tszProductName);

	allDevices.push_back(device);

	return DIENUM_CONTINUE;
}

BOOL CALLBACK EnumObjectsCallback(const DIDEVICEOBJECTINSTANCE* pdidoi, LPVOID pDeviceNum)
{
	int deviceNum = *(int*)pDeviceNum;

	if ((pdidoi->dwFlags & DIDOI_FFACTUATOR) != 0) {
		allDevices[deviceNum].hasForceFeedback = true;
	}

	//TODO: clean and explain
	if (pdidoi->dwType & DIDFT_AXIS)
	{
		DIPROPRANGE diprg;
		diprg.diph.dwSize = sizeof(DIPROPRANGE);
		diprg.diph.dwHeaderSize = sizeof(DIPROPHEADER);
		diprg.diph.dwHow = DIPH_BYID;
		diprg.diph.dwObj = pdidoi->dwType; // Specify the enumerated axis
		diprg.lMin = -32768;
		diprg.lMax = +32767;

		// Set the range for the axis
		if (FAILED(allDevices[deviceNum].deviceInterface->SetProperty(DIPROP_RANGE, &diprg.diph))) {
			return DIENUM_STOP;
		}
	}
	return DIENUM_CONTINUE;
}

BOOL CALLBACK EnumEffectsCallback(LPCDIEFFECTINFO effect, LPVOID pdeviceNum) {

	HRESULT              hr;
	int deviceNum = *(int*)pdeviceNum;

	LPDIRECTINPUTDEVICE8 lpdid = allDevices[deviceNum].deviceInterface;

	// Pointer to calling device
	LPDIRECTINPUTEFFECT  lpdiEffect;      // Pointer to created effect
	DIEFFECT             diEffect;        // Params for created effect
	DICONSTANTFORCE      diConstantForce; // Type-specific parameters

	//record effect name
	int effectNum = allDevices[deviceNum].num_effects++;
	_tcscpy_s(allDevices[deviceNum].effectNames[effectNum], _countof(allDevices[deviceNum].effectNames[effectNum]),effect->tszName);

	return DIENUM_CONTINUE;
}


extern "C" __declspec(dllexport) long Init() {
	HRESULT hr;

	//init DI
	if (FAILED(hr = DirectInput8Create(GetModuleHandle(NULL), DIRECTINPUT_VERSION,
		IID_IDirectInput8, (LPVOID*)&directInput, NULL))) {
		return hr;
	}

	allDevices.clear();
	//Enumerate Devices
	if (FAILED(hr = directInput->EnumDevices(DI8DEVCLASS_GAMECTRL, EnumDevicesCallback, NULL, DIEDFL_ATTACHEDONLY))) {
		return hr;
	}

	for (int d = 0; d < allDevices.size(); d++) {
		LPDIRECTINPUTDEVICE8 curDevice = allDevices[d].deviceInterface;

		//Enumerate objects to get FFB info and set axis ranges
		if (FAILED(hr = curDevice->EnumObjects(EnumObjectsCallback, &d, DIDFT_ALL))) {
			return hr;
		}

		//Set cooperative mode
		HWND hwnd = GetForegroundWindow();
		if (FAILED(hr = curDevice->SetCooperativeLevel(hwnd, DISCL_EXCLUSIVE | DISCL_BACKGROUND))) {
			return hr;
		}

		//set data format to basic DIJOYSTATE format
		if (FAILED(hr = curDevice->SetDataFormat(&c_dfDIJoystick))) {
			return hr;
		}

		//enumerate effects list
		if (FAILED(hr = curDevice->EnumEffects(&EnumEffectsCallback, &d, DIEFT_ALL))) {
			return hr;
		}

		if (allDevices[d].hasForceFeedback) {
			//set up damper and spring forces	
			DWORD rgdwAxes[1] = { DIJOFS_X };
			LONG rglDirection[1] = { 0 };
			DICONDITION cf = { 0 };

			DIEFFECT eff;
			ZeroMemory(&eff, sizeof(eff));
			eff.dwSize = sizeof(DIEFFECT);
			eff.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
			eff.dwDuration = INFINITE;
			eff.dwSamplePeriod = 0;
			eff.dwGain = DI_FFNOMINALMAX;
			eff.dwTriggerButton = DIEB_NOTRIGGER;
			eff.dwTriggerRepeatInterval = 0;
			eff.cAxes = 1;
			eff.rgdwAxes = rgdwAxes;
			eff.rglDirection = rglDirection;
			eff.lpEnvelope = 0;
			eff.cbTypeSpecificParams = sizeof(DICONDITION);
			eff.lpvTypeSpecificParams = &cf;
			eff.dwStartDelay = 0;

			if (FAILED(hr = curDevice->CreateEffect(GUID_Damper, &eff, &allDevices[d].damperForce, NULL))) {
				return hr;
			}
			if (FAILED(hr = curDevice->CreateEffect(GUID_Spring, &eff, &allDevices[d].springForce, NULL))) {
				return hr;
			}

			//set up constant force
			DICONSTANTFORCE constantForce;
			constantForce.lMagnitude = DI_FFNOMINALMAX;

			ZeroMemory(&eff, sizeof(eff));
			eff.dwSize = sizeof(DIEFFECT);
			eff.dwFlags = DIEFF_CARTESIAN | DIEFF_OBJECTOFFSETS;
			eff.dwDuration = INFINITE;
			eff.dwSamplePeriod = 0;
			eff.dwGain = DI_FFNOMINALMAX;
			eff.dwTriggerButton = DIEB_NOTRIGGER;
			eff.dwTriggerRepeatInterval = 0;
			eff.cAxes = 1;
			eff.rgdwAxes = rgdwAxes;
			eff.rglDirection = rglDirection;
			eff.lpEnvelope = 0;
			eff.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
			eff.lpvTypeSpecificParams = &constantForce;
			eff.dwStartDelay = 0;

			if (FAILED(hr = curDevice->CreateEffect(GUID_ConstantForce, &eff, &allDevices[d].constantForce, NULL))){
				return hr;
			}

		}

	}
	return 0;
}

extern "C" __declspec(dllexport) long __stdcall PlaySpringForce(int device, int offset, int saturation, int coefficient) {
	if (device >= allDevices.size()) {
		return ERROR_BAD_DEVICE_PATH;
	}

	DICONDITION params;
	params.lOffset = offset;
	params.lDeadBand = 0;
	params.dwPositiveSaturation = saturation;
	params.dwNegativeSaturation = saturation;
	params.lPositiveCoefficient = coefficient;
	params.lNegativeCoefficient = coefficient;

	DIEFFECT eff;
	ZeroMemory(&eff, sizeof(eff));
	eff.dwSize = sizeof(DIEFFECT);
	eff.cbTypeSpecificParams = sizeof(DICONDITION);
	eff.lpvTypeSpecificParams = &params;

	HRESULT hr;
	if (!allDevices[device].springForce) {
		return DIERR_UNSUPPORTED;
	}

	hr = allDevices[device].springForce->SetParameters(&eff, DIEP_TYPESPECIFICPARAMS | DIEP_START);
	// Now set the new parameters and start the effect immediately.
	if (FAILED(hr)) {
		return 1;
	}

	hr = allDevices[device].deviceInterface->Acquire();
	while (hr == DIERR_INPUTLOST) {
		hr = allDevices[device].deviceInterface->Acquire();
	}

	DWORD status = 0;
	
	if (FAILED(hr = allDevices[device].springForce->GetEffectStatus(&status))) {
		return hr;
	}

	if ((status & DIEGES_PLAYING) == 0) {
		if (FAILED(hr = allDevices[device].springForce->Start(-1, 0))) {
			return hr;
		}
	}
	return 0;

}

extern "C" __declspec(dllexport) bool __stdcall StopSpringForce(int device) {
	if (device >= allDevices.size() || !allDevices[device].springForce) {
		return false;
	}

	if (FAILED(allDevices[device].springForce->Stop())) {
		return false;
	}
	return true;
}


extern "C" __declspec(dllexport) long __stdcall PlayDamperForce(int device, int coefficient) {
	if (device >= allDevices.size()) {
		return ERROR_BAD_DEVICE_PATH;
	}

	if (!allDevices[device].damperForce) {
		return DIERR_UNSUPPORTED;
	}

	DICONDITION params;
	params.lOffset = 0;
	params.lDeadBand = 0;
	params.dwPositiveSaturation = 0;
	params.dwNegativeSaturation = 0;
	params.lPositiveCoefficient = coefficient;
	params.lNegativeCoefficient = coefficient;

	DIEFFECT eff;
	ZeroMemory(&eff, sizeof(eff));
	eff.dwSize = sizeof(DIEFFECT);
	eff.cbTypeSpecificParams = sizeof(DICONDITION);
	eff.lpvTypeSpecificParams = &params;

	HRESULT hr;
	hr = allDevices[device].damperForce->SetParameters(&eff, DIEP_TYPESPECIFICPARAMS | DIEP_START);

	// Now set the new parameters and start the effect immediately.
	if (FAILED(hr)) {
		return 1;
	}

	hr = allDevices[device].deviceInterface->Acquire();
	while (hr == DIERR_INPUTLOST) {
		hr = allDevices[device].deviceInterface->Acquire();
	}

	DWORD status = 0;

	if (FAILED(hr = allDevices[device].damperForce->GetEffectStatus(&status))) {
		return hr;
	}

	if ((status & DIEGES_PLAYING) == 0) {
		if (FAILED(hr = allDevices[device].damperForce->Start(-1, 0))) {
			return hr;
		}
	}

	return 0;
}

extern "C" __declspec(dllexport) bool __stdcall StopDamperForce(int device) {
	if (device >= allDevices.size() || !allDevices[device].damperForce) {
		return false;
	}

	if (FAILED(allDevices[device].damperForce->Stop())) {
		return false;
	}
	return true;
}

extern "C" __declspec(dllexport) long __stdcall PlayConstantForce(int device, int force) {
	if (device >= allDevices.size()) {
		return ERROR_BAD_DEVICE_PATH;
	}
	if (!allDevices[device].constantForce) {
		return DIERR_UNSUPPORTED;
	}

	DICONSTANTFORCE params;
	params.lMagnitude = force;

	DIEFFECT eff;
	ZeroMemory(&eff, sizeof(eff));
	eff.dwSize = sizeof(DIEFFECT);
	eff.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	eff.lpvTypeSpecificParams = &params;

	HRESULT hr;

	hr = allDevices[device].constantForce->SetParameters(&eff, DIEP_TYPESPECIFICPARAMS | DIEP_START);
	// Now set the new parameters and start the effect immediately.
	if (FAILED(hr)) {
		return hr;
	}

	hr = allDevices[device].deviceInterface->Acquire();
	while (hr == DIERR_INPUTLOST) {
		hr = allDevices[device].deviceInterface->Acquire();
	}

	DWORD status = 0;

	if (FAILED(hr = allDevices[device].constantForce->GetEffectStatus(&status))) {
		return hr;
	}

	if ((status & DIEGES_PLAYING) == 0) {
		if (FAILED(hr = allDevices[device].constantForce->Start(-1, 0))) {
			return hr;
		}
	}
	return 0;

}


extern "C" __declspec(dllexport) long __stdcall UpdateConstantForce(int device, int force) {
	if (device >= allDevices.size()) {
		return ERROR_BAD_DEVICE_PATH;
	}
	if (!allDevices[device].constantForce) {
		return DIERR_UNSUPPORTED;
	}

	DICONSTANTFORCE params;
	params.lMagnitude = force;

	DIEFFECT eff;
	eff.dwSize = sizeof(DIEFFECT);
	eff.cbTypeSpecificParams = sizeof(DICONSTANTFORCE);
	eff.lpvTypeSpecificParams = &params;

	HRESULT hr;
	hr = allDevices[device].constantForce->SetParameters(&eff, DIEP_TYPESPECIFICPARAMS | DIEP_NORESTART);
	// Now set the new parameters and start the effect immediately.
	if (FAILED(hr)) {
		return hr;
	}

	return 0;
}

extern "C" __declspec(dllexport) bool __stdcall StopConstantForce(int device) {
	if (device >= allDevices.size() || !allDevices[device].constantForce) {
		return false;
	}

	if (FAILED(allDevices[device].constantForce->Stop())) {
		return false;
	}
	return true;
}


extern "C" __declspec(dllexport) void Update() {

	HRESULT hr;
	for each (auto device in allDevices) {
		hr = device.deviceInterface->Poll();
		if (FAILED(hr))
		{
			hr = device.deviceInterface->Acquire();
			while (hr == DIERR_INPUTLOST) {
				hr = device.deviceInterface->Acquire();
			}
		}
	}
}

extern "C" __declspec(dllexport) DIJOYSTATE* GetState(int device) {
	if (device >= allDevices.size())
	{
		return NULL;
	}

	DIJOYSTATE* state = new DIJOYSTATE();
	HRESULT hr;
	if (FAILED(hr = allDevices[device].deviceInterface->GetDeviceState(sizeof(DIJOYSTATE), state)))
		return NULL; // The device should have been acquired during the Poll()

	return state;
}

extern "C" __declspec(dllexport) TCHAR* GetProductName(int device) {
	if (device >= allDevices.size())
	{
		return _T("");
	}
	else {
		return allDevices[device].productName;
	}
}

extern "C" __declspec(dllexport) int GetNumEffects(int device) {
	if (device >= allDevices.size())
	{
		return NULL;
	}
	else
	{
		return allDevices[device].num_effects;
	}
}

extern "C" __declspec(dllexport) TCHAR* GetEffectName(int device, int index) {
	if (device >= allDevices.size())
	{
		return NULL;
	}
	else {
		return allDevices[device].effectNames[index];
	}
}

extern "C" __declspec(dllexport) int DevicesCount() {
	return allDevices.size();
}

extern "C" __declspec(dllexport) bool HasForceFeedback(int device) {
	if (device >= allDevices.size())
	{
		return false;
	}
	else {
		return allDevices[device].hasForceFeedback;
	}
}

extern "C" __declspec(dllexport) void Close() {

	for (auto device : allDevices) {
		if (device.constantForce) {
			device.constantForce->Release();
			device.constantForce = NULL;
		}
		if (device.damperForce) {
			device.damperForce->Release();
			device.damperForce = NULL;
		}
		if (device.springForce) {
			device.springForce->Release();
			device.springForce = NULL;
		}
		if (device.deviceInterface) {
			device.deviceInterface->Unacquire();
			device.deviceInterface->Release();
			device.deviceInterface = NULL;
		}
	}

	if (directInput) {
		directInput->Release();
		directInput = NULL;
	}

	allDevices.clear();
}