//yuv420sp2rgb.h

JNIEXPORT void JNICALL Java_org_secmem232_cloudphone_CameraPreview_yuv420sp2rgb
  (JNIEnv* env, jobject object, jbyteArray pinArray, jint width, jint height, jint textureSize, jbyteArray poutArray);

static inline void color_convert_common(
    unsigned char *pY, unsigned char *pUV,
    int width, int height, int texture_size,
    unsigned char *buffer,
    int size, /* buffer size in bytes */
    int gray,
    int rotate);
