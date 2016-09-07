import socket
import struct

sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(("127.0.0.1", 9000))

while True:
	data = sock.recv(1024)
	if len(data) > 0:
		floats = struct.unpack('16f', data)
		print "EMSSetSpeed, %f, %f\n" \
				"EngineSpeed, %f, %f\n" \
				"GearPosActual, %f, %f\n" \
				"GearPosTarget, %f, %f\n" \
				"AcceleratorPedalPos, %f, %f\n" \
				"DeceleratorPedalPos, %f, %f\n" \
				"RollRate, %f, %f\n" \
				"SteeringWheelAngle, %f, %f\n" \
				"VehicleSpeed, %f, %f\n" \
				"VehicleSpeedOverGnd, %f, %f\n" \
				"WheelSpeedFrL, %f, %f\n" \
				"WheelSpeedFrR, %f, %f\n" \
				"WheelSpeedReL, %f, %f\n" \
				"WheelSpeedReR, %f, %f\n" \
				"YawRate, %f, %f" \
				% (floats[1], floats[0], \
				floats[2], floats[0], \
				floats[3], floats[0], \
				floats[4], floats[0], \
				floats[5], floats[0], \
				floats[6], floats[0], \
				floats[7], floats[0], \
				floats[8], floats[0], \
				floats[9], floats[0], \
				floats[10], floats[0], \
				floats[11], floats[0], \
				floats[12], floats[0], \
				floats[13], floats[0], \
				floats[14], floats[0], \
				floats[15], floats[0])
