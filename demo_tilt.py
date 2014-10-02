#!/usr/bin/env python
import freenect
import time
import random
import signal

keep_running = True
last_time = 0


def body(dev, ctx):
    global last_time
    if not keep_running:
        raise freenect.Kill
    if time.time() - last_time < 3:
        return
    print "boop"
    last_time = time.time()
    led = random.randint(0, 6)
    tilt = random.randint(0, 30)
    print "set_led(%s)" % led
    freenect.set_led(dev, led)
    print "set_tilt_degs(%s)" % tilt
    freenect.set_tilt_degs(dev, tilt)
    print('led[%d] tilt[%d] accel[%s]' % (led, tilt, freenect.get_accel(dev)))

def fake_display(dev, data, timestamp):
  return


def handler(signum, frame):
    """Sets up the kill handler, catches SIGINT"""
    global keep_running
    keep_running = False
#print('Press Ctrl-C in terminal to stop')
#signal.signal(signal.SIGINT, handler)
freenect.runloop(body=body, depth=fake_display, video=fake_display)
