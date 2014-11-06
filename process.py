import numpy as np
import sys, cv2, freenect
from functools import partial

KEY_UP = 63232
KEY_DN = 63233

do_edge = True

print "Reloaded!"



def print_trackbar(value):
  print "%s" % value

#min:11, max:26
cv2.createTrackbar('minVal', 'Depth', 0, 255, print_trackbar)
cv2.createTrackbar('maxVal', 'Depth', 0, 255, print_trackbar)

def clip(d):
  np.clip(d, 0, 2**10 - 1, d)
  d >>= 2
  d = d.astype(np.uint8)
  return d

def depth_func(d, k):
  global do_edge

  #sys.stdout.write('[%s]' % k)
  #sys.stdout.flush()

  if k == ord('e'):
    print "swap do_edge"
    do_edge = not do_edge
  elif k != '':
    print "key: %s" % k

  d = clip(d)
  #d = np.clip(d, 130, 140, d)
  if do_edge:
    return cv2.Canny(d, cv2.getTrackbarPos('minVal', 'Depth'),
                        cv2.getTrackbarPos('maxVal', 'Depth'))
  return d
