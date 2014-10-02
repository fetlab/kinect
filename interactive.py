#!/usr/bin/env python
import freenect, cv2
import sys, imp
import numpy as np

KEY_UP = 63232
KEY_DN = 63233

#Function to call for each frame of depth data
depth_func  = lambda x: x
module_path = ''
module_key  = ''

device = None

cv2.namedWindow('Depth')
cv2.namedWindow('RGB')
keep_running = True

cv2.setMouseCallback('Depth', mouse, 'Depth')
cv2.setMouseCallback('RGB',   mouse, 'RGB')


#def mouse(e, x, y, flags, param):
  #if e == cv2.EVENT_LBUTTONUP:


def load_external_module(fname):
  try:
    user_code = imp.load_source('user_code', fname)
  except Exception, e:
    sys.stderr.write(str(e))
  else:
    try:
      global depth_func
      depth_func = user_code.depth_func
    except AttributeError:
      sys.stderr.write("ERROR: Module at %s must have a depth_func() function!" % fname)


def process_keys(dev):
  global keep_running, module_key
  k = cv2.waitKey(10)
  if k == ord('q'):
    keep_running = False
  elif k == ord('r'):
    load_external_module(module_path)
  elif k != -1:
    print "Key: %s" % k
    module_key = k


def display_depth(dev, data, timestamp):
  #np.clip(data, 0, 2**10 - 1, data)
  #data >>= 2
  #data = data.astype(np.uint8)
  global module_key
  try:
    d = depth_func(data, module_key)
    module_key = ''
  except Exception, e:
    cv2.imshow('Depth', data)
  else:
    cv2.imshow('Depth', d)
  process_keys(dev)


def display_rgb(dev, data, timestamp):
  cv2.imshow('RGB', cv2.cvtColor(data, cv2.cv.CV_BGR2RGB))
  process_keys(dev)


def main(dev, ctx):
  if not keep_running:
    raise freenect.Kill
  #depth, timestamp = freenect.sync_get_depth()
  #display_depth(depth, timestamp)
  #display_rgb(*freenect.sync_get_video())


if __name__ == "__main__":
  import sys

  if not sys.argv[1:] or sys.argv[1][:2] == '-h':
    print "Usage: %s -h | module.py" % sys.argv[0]
    print "  -h:        Show this help"
    print "  module.py: Load code from module, use 'r' while running to reload"
    sys.exit(0)

  module_path = sys.argv[1]
  load_external_module(module_path)

freenect.runloop(depth=display_depth, video=display_rgb, body=main)
