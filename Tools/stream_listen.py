import socket
import threading

bind_ip = '0.0.0.0'
bind_port = 9002

server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
server.bind((bind_ip, bind_port))
server.listen(5)  # max backlog of connections

print 'Listening on {}:{}'.format(bind_ip, bind_port)


def handle_client_connection(client_socket):
	while True:
		try:
			request = client_socket.recv(1024)
			print 'Received {}'.format(request)
		except KeyboardInterrupt:
			if client_socket:  # <---
				client_socket.close()
			break 

while True:
    client_sock, address = server.accept()
    print 'Accepted connection from {}:{}'.format(address[0], address[1])
    client_handler = threading.Thread(
        target=handle_client_connection,
        args=(client_sock,)  # without comma you'd get a... TypeError: handle_client_connection() argument after * must be a sequence, not _socketobject
    )
    client_handler.start()