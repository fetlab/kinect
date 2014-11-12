import numpy as np
import sys, cv2, freenect
from functools import partial
from danutil import *

KEY_UP = 63232
KEY_DN = 63233

do_depth_edge = True
do_rgb_edge = False
do_blur = True

print "Reloaded!"

rgb_image = None



def print_trackbar(value):
  print "%s" % value

cv2.createTrackbar('clipNear', 'Depth', 0, 2**10-1, print_trackbar)
cv2.createTrackbar('clipFar', 'Depth', 2**10-1, 2**10-1, print_trackbar)
#min:11, max:26
cv2.createTrackbar('minVal', 'Depth', 0, 255, print_trackbar)
cv2.createTrackbar('maxVal', 'Depth', 0, 255, print_trackbar)
cv2.createTrackbar('minVal', 'RGB', 0, 255, print_trackbar)
cv2.createTrackbar('maxVal', 'RGB', 0, 255, print_trackbar)

def clip(d):
  mindepth = cv2.getTrackbarPos('clipNear', 'Depth')
  maxdepth = cv2.getTrackbarPos('clipFar', 'Depth')
  
  d[np.where(d <= mindepth)] = 0
  d[np.where(d >= maxdepth)] = 0

  #np.clip(d, cv2.getTrackbarPos('clipMin', 'Depth'),
      #cv2.getTrackbarPos('clipMax', 'Depth'), d)
  d >>= 2
  d = d.astype(np.uint8)
  return d


def rgb_func(d):
  global rgb_image
  rgb_image = d
  if do_blur:
    d = cv2.GaussianBlur(d, (5,5), 0)
  if do_rgb_edge:
    d = cv2.Canny(d, cv2.getTrackbarPos('minVal', 'RGB'),
                     cv2.getTrackbarPos('maxVal', 'RGB'))
  return d


def depth_func(d, k):
  global do_depth_edge, do_rgb_edge, do_blur

  #sys.stdout.write('[%s]' % k)
  #sys.stdout.flush()

  if k == ord('e'):
    print "swap do_depth_edge"
    do_depth_edge = not do_depth_edge
  elif k == ord('f'):
    print "swap do_rgb_edge"
    do_rgb_edge = not do_rgb_edge
  elif k == ord('p'):
    pickle({'rgb': rgb_image, 'depth': d}, '/tmp/kinect.pickle')
    print "pickled to /tmp/kinect.pickle"
  elif k == ord('b'):
    do_blur = not do_blur
  elif k != '':
    print "key: %s" % k

  d = clip(d)
  if do_blur:
    d = cv2.GaussianBlur(d, (5,5), 0)
  #d = np.clip(d, 130, 140, d)
  if do_depth_edge:
    e = cv2.Canny(d, cv2.getTrackbarPos('minVal', 'Depth'),
                     cv2.getTrackbarPos('maxVal', 'Depth'))
    return e
    return where(e == 0, [d, e])
  return d
