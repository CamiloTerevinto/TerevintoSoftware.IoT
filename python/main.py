from machine import Pin, I2C, reset
from dht import DHT22
from ssd1306 import SSD1306_I2C
from requests import post
from utime import sleep
from config import configuration

def connect(oled):
    from network import WLAN, STA_IF

    wlan = WLAN(STA_IF)
    wlan.active(True)
    wlan.connect(configuration["wifi_name"], configuration["wifi_password"])
    time_taken = 1
    
    while wlan.isconnected() == False and time_taken <= 30:
        oled.fill(0)
        oled.text(f"Connecting... {time_taken}s", 0, 10)
        oled.show()
        sleep(1)
        time_taken += 1
    
    if wlan.isconnected() == False:
        oled.fill(0)
        oled.text("Could not connect!", 0, 10)
        oled.text("Restarting...", 0, 20)
        oled.show()
        sleep(5)
        reset()
    
    return wlan

def measure_and_store(wlan, oled, measurements):
    sensor = DHT22(dht_pin)
    sensor.measure()
    
    if wlan.isconnected() == False:
        wlan = connect(oled)
        
        if wlan.isconnected() == False:
            raise Exception("Could not connect")
        else:
            oled.show()
    
    oled.fill(0)
    oled.text(f'Count: {measurements}', 0, 10)
    oled.text(f'Temp: {sensor.temperature()}C', 0, 20)
    oled.text(f"Humidity: {sensor.humidity()}%", 0, 30)
    oled.show()
    
    req_body = f'{{ "Humidity": {sensor.humidity()}, "Temperature": {sensor.temperature()} }}'
    post(configuration["api_url"], headers = {'content-type': 'application/json'}, data=req_body)

WIDTH = 128
HEIGHT = 64

i2c = I2C(0, scl=Pin(1), sda=Pin(0), freq=200000)
dht_pin = Pin(3, Pin.OUT, Pin.PULL_DOWN)

oled = SSD1306_I2C(WIDTH, HEIGHT, i2c)
oled.fill(0)
oled.text("Welcome :)", 0, 10)
oled.show()

wlan = connect(oled)
measurements = 1

while True:
    try:
        measure_and_store(wlan, oled, measurements)
        measurements += 1
        sleep(configuration["frequency"])
    except Exception as ex:
        oled.fill(0)
        oled.text("Error :(", 0, 10)
        oled.text(str(ex), 0, 20)
        oled.text("Restarting...", 0, 30)
        oled.show()
        sleep(30)
        reset()