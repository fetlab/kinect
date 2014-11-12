#!/usr/bin/env python
import freenect, cv2
import sys, imp
import numpy as np


class Kinect(object):
  KEY_UP = 63232
  KEY_DN = 63233

  def __init__(self, module_path=None):
    self.module_path = module_path
    self.depth_func = lambda x: x
    self.module_key = ''
    self.keep_running = True
    self.depth_func = None
    self.depth_image = None

    if self.module_path:
      self.load_external_module(self.module_path)

    cv2.namedWindow('Depth')
    cv2.setMouseCallback('Depth', self.mouse_depth)

    cv2.namedWindow('RGB')
    cv2.setMouseCallback('RGB', self.mouse_rgb)

    freenect.runloop(depth=self.display_depth,
        video=self.display_rgb,
        body=self.main_loop)


  def mouse_depth(self, e, x, y, flags, param):
    #print e, x, y, flags, param
    if e == cv2.EVENT_LBUTTONUP:
      if self.depth_image is not None:
        print self.depth_image[y,x]

  
  def mouse_rgb(self, e, x, y, flags, param):
    pass


  def load_external_module(self, fname):
    try:
      user_code = imp.load_source('user_code', fname)
    except Exception, e:
      sys.stderr.write(str(e))
    else:
      try:
        self.depth_func = user_code.depth_func
        self.rgb_func   = user_code.rgb_func
      except AttributeError:
        sys.stderr.write(("ERROR: Module at %s must have depth_func() "
            "and rgb_func() functions!") % fname)


  def process_keys(self):
    k = cv2.waitKey(10)
    if k == ord('q'):
      self.keep_running = False
    elif k == ord('r'):
      self.load_external_module(self.module_path)
    elif k != -1:
      print "Key: %s" % k
      self.module_key = k


  def display_depth(self, dev, data, timestamp):
    self.depth_image = data
    try:
      d = self.depth_func(data, self.module_key)
      self.module_key = ''
    except Exception, e:
      print "Exception: %s" % e
      cv2.imshow('Depth', data)
    else:
      cv2.imshow('Depth', d)
    self.process_keys()


  def display_rgb(self, dev, data, timestamp):
    data = cv2.cvtColor(data, cv2.cv.CV_BGR2RGB)
    try:
      d = self.rgb_func(data)
    except Exception, e:
      cv2.imshow('RGB', data)
    else:
      cv2.imshow('RGB', d)
    self.process_keys()


  def main_loop(self, dev, ctx):
    if not self.keep_running:
      raise freenect.Kill


if __name__ == "__main__":
  import sys

  if not sys.argv[1:] or sys.argv[1][:2] == '-h':
    print "Usage: %s -h | module.py" % sys.argv[0]
    print "  -h:        Show this help"
    print "  module.py: Load code from module, use 'r' while running to reload"
    sys.exit(0)

  module_path = sys.argv[1]
  k = Kinect(module_path)
