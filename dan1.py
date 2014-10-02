import freenect
import cv2
import frame_convert
import numpy as np
import cPickle

i = 0

def getDepthMap():
  global i
  i += 1
  d,t = freenect.sync_get_depth()
  np.clip(d, 0, 2**10 - 1, d)
  d >>= 2
  d = d.astype(np.uint8)
  return d

while True:
  d = getDepthMap()
  b = cv2.GaussianBlur(d, (5,5), 0)
  cv2.imshow('image', b)
  if cv2.waitKey(10) == 'q' or i > 101:
    break
