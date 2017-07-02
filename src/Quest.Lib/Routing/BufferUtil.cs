using System;

namespace Quest.Lib.Routing
{
    public class BufferUtil
    {
        // The unsafe keyword allows pointers to be used within
        // the following method:
        public static unsafe void Clear(byte[] src)
        {
            if (src == null)
            {
                throw new ArgumentException();
            }

            var srcLen = src.Length;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src)
            {
                var ps = pSrc;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < srcLen/4; n++, ps += 4)
                    *(int*) ps = 0;

                // Complete the copy by moving any bytes that weren't
                // moved in blocks of 4:
                for (var n = 0; n < srcLen%4; n++, ps++)
                    *ps = 0;
            }
        }

        public static unsafe void Set(byte[] src, byte value)
        {
            if (src == null)
                throw new ArgumentException();

            var srcLen = src.Length;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src)
            {
                var ps = pSrc;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < srcLen; n++, ps++)
                    *ps = value;
            }
        }

        /// <summary>
        ///     Merge the mergemap into the basemap. take only values from mergemap that are lower than in basemap.
        ///     ** zero values in the mergmap are ignored **
        /// </summary>
        /// <param name="basemap">The target map to be updated</param>
        /// <param name="mergemap">the map to me merged into thetarget map</param>
        public static unsafe void MergeMin(byte[] basemap, byte[] mergemap)
        {
            if (basemap == null || mergemap == null || basemap.Length != mergemap.Length)
                throw new ArgumentException();

            var srcLen = basemap.Length;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = mergemap, pDst = basemap)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < srcLen; n++, pd++, ps++)
                {
                    if (*ps != 0)
                        if (*ps < *pd || *pd == 0)
                            *pd = *ps;
                }
            }
        }


        public static unsafe void CopyAsValue(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count, byte value)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            var dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < count; n++, pd++, ps++)
                {
                    *pd = (byte) (*ps > 0 ? value : 0);
                }
            }
        }

        /// <summary>
        ///     set destination cell as value where source cell is >0. If source cell=0 leave destination untouched
        /// </summary>
        /// <param name="src"></param>
        /// <param name="srcIndex"></param>
        /// <param name="dst"></param>
        /// <param name="dstIndex"></param>
        /// <param name="count"></param>
        /// <param name="value"></param>
        public static unsafe void MergeAsValue(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count, byte value)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            var dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < count; n++, pd++, ps++)
                {
                    if (*ps > 0)
                        *pd = value;
                }
            }
        }

        // The unsafe keyword allows pointers to be used within
        // the following method:
        public static unsafe bool IsDifferent(byte[] src, byte[] dst)
        {
            if (src == null || dst == null || src.Length != dst.Length)
            {
                return true;
            }

            var srcLen = src.Length;
            var dstLen = dst.Length;


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < srcLen/4; n++)
                {
                    if (*(int*) pd != *(int*) ps)
                        return true;
                    pd += 4;
                    ps += 4;
                }

                // Complete the copy by moving any bytes that weren't
                // moved in blocks of 4:
                for (var n = 0; n < srcLen%4; n++)
                {
                    if (*pd != *ps)
                        return true;
                    pd++;
                    ps++;
                }
            }

            return false;
        }


        // The unsafe keyword allows pointers to be used within
        // the following method:
        public static unsafe void Copy(byte[] src, int srcIndex,
            byte[] dst, int dstIndex, int count)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            var dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < count/4; n++)
                {
                    *(int*) pd = *(int*) ps;
                    pd += 4;
                    ps += 4;
                }

                // Complete the copy by moving any bytes that weren't
                // moved in blocks of 4:
                for (var n = 0; n < count%4; n++)
                {
                    *pd = *ps;
                    pd++;
                    ps++;
                }
            }
        }

        // The unsafe keyword allows pointers to be used within
        // the following method:
        public static unsafe void Add(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            var dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < count; n++, pd++, ps++)
                    *pd += *ps;
            }
        }

        // The unsafe keyword allows pointers to be used within
        // the following method:
        public static unsafe void Subtract(byte[] src, int srcIndex, byte[] dst, int dstIndex, int count)
        {
            if (src == null || srcIndex < 0 ||
                dst == null || dstIndex < 0 || count < 0)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            var dstLen = dst.Length;
            if (srcLen - srcIndex < count ||
                dstLen - dstIndex < count)
            {
                throw new ArgumentException();
            }


            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src, pDst = dst)
            {
                var ps = pSrc;
                var pd = pDst;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < count; n++, pd++, ps++)
                    *pd -= *ps;
            }
        }

        public static unsafe long Multiply(byte[] src1, byte[] src2)
        {
            if (src1 == null || src2 == null)
            {
                throw new ArgumentException();
            }
            var src1Len = src1.Length;
            var src2Len = src1.Length;
            if (src1Len != src2Len)
            {
                throw new ArgumentException();
            }

            long total = 0;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc1 = src1, pSrc2 = src2)
            {
                var ps1 = pSrc1;
                var ps2 = pSrc2;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < src1Len; n++, ps1++, ps2++)
                    total += *ps1**ps2;
            }

            return total;
        }

        public static unsafe long Sum(byte[] src)
        {
            if (src == null)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            long total = 0;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src)
            {
                var ps = pSrc;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < srcLen; n++, ps++)
                    total += *ps;
            }
            return total;
        }

        public static unsafe int CountNonZero(byte[] src)
        {
            if (src == null)
            {
                throw new ArgumentException();
            }
            var srcLen = src.Length;
            var total = 0;

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc = src)
            {
                var ps = pSrc;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < srcLen; n++, ps++)
                {
                    if (*ps > 0)
                        total += 1;
                }
                //                    total = *ps>0?1:0;
            }
            return total;
        }


        public static unsafe void Move(byte[] before, byte[] after, byte[] target)
        {
            if (before == null || after == null || target == null)
            {
                throw new ArgumentException();
            }
            var beforeLen = before.Length;
            var afterLen = after.Length;
            var targetLen = target.Length;

            if (beforeLen != afterLen)
            {
                throw new ArgumentException();
            }

            if (beforeLen != targetLen)
            {
                throw new ArgumentException();
            }

            // The following fixed statement pins the location of
            // the src and dst objects in memory so that they will
            // not be moved by garbage collection.          
            fixed (byte* pSrc1 = before, pSrc2 = after, pTarget = target)
            {
                var ps1 = pSrc1;
                var ps2 = pSrc2;
                var ps3 = pTarget;

                // Loop over the count in blocks of 4 bytes, copying an
                // integer (4 bytes) at a time:
                for (var n = 0; n < beforeLen; n++, ps1++, ps2++, ps3++)
                {
                    var diff = *ps2 - *ps1;
                    if (diff > 0)
                        *ps3 += 1;

                    if (diff < 0)
                        *ps3 -= 1;
                }
            }
        }
    }
}