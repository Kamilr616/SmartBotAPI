import websocket as ws_client
import ssl
import websockets
import asyncio
import random

MODE = "async"  # Change to "sync" for synchronous mode


class WebSocketTester:
    def __init__(self, url):
        self.url = url
        self.ssl_context = ssl.SSLContext(ssl.PROTOCOL_TLS_CLIENT)
        self.ssl_context.check_hostname = False
        self.ssl_context.verify_mode = ssl.CERT_NONE
        self.image_width = 8  # Example width of the depth image (e.g., 8x8 image)

    # Synchronous WebSocket Client using websocket-client
    def on_open(self, ws):
        print("Connected to WebSocket.")
        self.send_binary_data(ws)

    def on_message(self, ws, message):
        print(f"Message from server: {message}")

    def on_close(self, ws, close_status_code, close_msg):
        print(f"Disconnected from WebSocket. Code: {close_status_code}, Reason: {close_msg}")

    def on_error(self, ws, error):
        print(f"WebSocket error: {error}")

    def send_binary_data(self, ws):
        # Simulate binary data similar to depth image (e.g., 128 bytes)
        data, distances = self.generate_depth_image()
        #print(f"Sending binary data: {data}")
        print(f"Distances (mm): {distances}")  # Print the distance values
        ws.send(data, opcode=ws_client.ABNF.OPCODE_BINARY)

    def generate_depth_image(self):
        # Simulate creating a 128-byte depth image (8x8 pixels)
        data = bytearray(128)
        distances = []  # Store distances to print
        z = 0

        for y in range(0, self.image_width * (self.image_width - 1) + 1, self.image_width):
            for x in range(self.image_width - 1, -1, -1):
                # Simulate depth data with values between 0 and 4000 mm
                distance = random.randint(0, 4000)
                distances.append(distance)  # Add distance to the list

                # Pack the low and high bytes in one go
                data[z] = distance & 0xFF  # Low byte
                data[z + 1] = (distance >> 8) & 0xFF  # High byte
                z += 2

        return data, distances

    def run_synchronous_client(self):
        print("Connecting to WebSocket server (sync)...")
        ws = ws_client.WebSocketApp(self.url,
                                    on_open=self.on_open,
                                    on_message=self.on_message,
                                    on_close=self.on_close,
                                    on_error=self.on_error
                                    )
        ws_client.enableTrace(True)

        try:
            # Run the WebSocket connection synchronously
            ws.run_forever(sslopt={"cert_reqs": 0})
        except Exception as e:
            print(f"Failed to run WebSocket (sync): {e}")

    # Asynchronous WebSocket Client using websockets and asyncio
    async def run_asynchronous_client(self):
        print("Connecting to WebSocket server (async)...")
        try:
            async with websockets.connect(self.url, ssl=self.ssl_context) as websocket:
                print("Connected to WebSocket.")
                while True:
                    data, distances = self.generate_depth_image()
                    await websocket.send(data)
                    #await websocket.send("Test message from Python!")
                    #print(f"Sent binary data: {data}")
                    print(f"Distances (mm): {distances}")  # Print the distance values
                    await asyncio.sleep(0.07)  # Wait between transmissions
        except Exception as e:
            print(f"Failed to run WebSocket (async): {e}")

    def run(self, mode="sync"):
        if mode == "sync":
            self.run_synchronous_client()
        elif mode == "async":
            # Run the asynchronous WebSocket client
            asyncio.run(self.run_asynchronous_client())
        else:
            print("Invalid mode! Use 'sync' or 'async'.")


# Usage
if __name__ == "__main__":
    websocket_url =  "wss://localhost:32773/api/WebSocket/ws" #"wss://smartbotapi.azurewebsites.net/api/WebSocket/ws"

    tester = WebSocketTester(websocket_url)
    tester_mode = MODE
    tester.run(mode=tester_mode)
