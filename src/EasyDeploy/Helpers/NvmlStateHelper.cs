using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EasyDeploy.Helpers
{
    /// <summary>
    /// 显卡利用率统计
    /// </summary>
    public class NvmlStateHelper
    {
        /// <summary>
        /// NVML 库名称
        /// </summary>
        const string NVML_SHARED_LIBRARY_STRING = "nvml.dll";

        /// <summary>
        /// Buffer size guaranteed to be large enough for \ref nvmlDeviceGetName
        /// </summary>
        const int NVML_DEVICE_NAME_BUFFER_SIZE = 64;

        /// <summary>
        /// Buffer size guaranteed to be large enough for \ref nvmlDeviceGetUUID
        /// </summary>
        const int NVML_DEVICE_UUID_BUFFER_SIZE = 80;

        /// <summary>
        /// 初始化 NVML 库
        /// </summary>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, EntryPoint = "nvmlInit_v2")]
        internal static extern NvmlReturn NvmlInitV2();

        /// <summary>
        /// 获取显卡数量
        /// </summary>
        /// <param name="deviceCount"></param>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, CharSet = CharSet.Ansi, EntryPoint = "nvmlDeviceGetCount_v2")]
        internal static extern NvmlReturn NvmlDeviceGetCount_v2(out uint deviceCount);

        /// <summary>
        /// 获取显卡句柄
        /// </summary>
        /// <param name="index"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, EntryPoint = "nvmlDeviceGetHandleByIndex")]
        internal static extern NvmlReturn NvmlDeviceGetHandleByIndex(uint index, out IntPtr device);

        /// <summary>
        /// 获取显卡名称
        /// </summary>
        /// <param name="device"></param>
        /// <param name="name"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, CharSet = CharSet.Ansi, EntryPoint = "nvmlDeviceGetName")]
        internal static extern NvmlReturn NvmlDeviceGetName(IntPtr device, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] name, uint length);

        /// <summary>
        /// 获取显卡 UUID
        /// </summary>
        /// <param name="device"></param>
        /// <param name="uuid"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, CharSet = CharSet.Ansi, EntryPoint = "nvmlDeviceGetUUID")]
        internal static extern NvmlReturn NvmlDeviceGetUUID(IntPtr device, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] uuid, uint length);

        /// <summary>
        /// 获取显卡使用率信息
        /// </summary>
        /// <param name="device"></param>
        /// <param name="utilization"></param>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, CharSet = CharSet.Ansi, EntryPoint = "nvmlDeviceGetUtilizationRates")]
        internal static extern NvmlReturn NvmlDeviceGetUtilizationRates(IntPtr device, out NvmlUtilization utilization);

        /// <summary>
        /// 关闭调用
        /// </summary>
        /// <returns></returns>
        [DllImport(NVML_SHARED_LIBRARY_STRING, EntryPoint = "nvmlShutdown")]
        internal static extern NvmlReturn NvmlShutdown();

        /// <summary>
        /// 获取显卡全局使用率
        /// </summary>
        /// <param name="gpuCount"></param>
        /// <exception cref="SystemException"></exception>
        public NvmlStateHelper(uint gpuCount = 0)
        {
            Task.Run(() =>
            {
                NvmlReturn res = NvmlInitV2();
                if (NvmlReturn.NVML_SUCCESS != res)
                {
                    //throw new SystemException(res.ToString());
                    return;
                }

                var device = IntPtr.Zero;
                res = NvmlDeviceGetHandleByIndex(gpuCount, out device);
                if (NvmlReturn.NVML_SUCCESS != res)
                {
                    //throw new SystemException(res.ToString());
                    return;
                }

                // 获取显卡名称
                byte[] bufferName = new byte[NVML_DEVICE_NAME_BUFFER_SIZE];
                res = NvmlDeviceGetName(device, bufferName, NVML_DEVICE_NAME_BUFFER_SIZE);
                if (NvmlReturn.NVML_SUCCESS == res)
                {
                    var gpuName = Encoding.Default.GetString(bufferName).Replace("\0", "");
                }

                // 获取显卡 UUID
                byte[] bufferUUID = new byte[NVML_DEVICE_UUID_BUFFER_SIZE];
                res = NvmlDeviceGetUUID(device, bufferUUID, NVML_DEVICE_UUID_BUFFER_SIZE);
                if (NvmlReturn.NVML_SUCCESS == res)
                {
                    var gpuUUID = Encoding.Default.GetString(bufferUUID).Replace("\0", "");
                }

                // 获取 GPU 与显存使用率
                while (true)
                {
                    try
                    {
                        NvmlUtilization nvmlUtilization;
                        res = NvmlDeviceGetUtilizationRates(device, out nvmlUtilization);
                        if (NvmlReturn.NVML_SUCCESS != res)
                        {
                            //throw new SystemException(res.ToString());
                            return;
                        }

                        GpuChange?.Invoke(nvmlUtilization.gpu);
                        MemoryChange?.Invoke(nvmlUtilization.memory);
                    }
                    catch (Exception)
                    {
                        // 获取信息失败
                    }

                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// Gpu 使用率
        /// </summary>
        public event Action<uint>? GpuChange;

        /// <summary>
        /// 显存使用率
        /// </summary>
        public event Action<uint>? MemoryChange;
    }

    /// <summary>
    /// NVML 返回值类型
    /// </summary>
    public enum NvmlReturn
    {
        NVML_SUCCESS = 0,
        NVML_ERROR_UNINITIALIZED,
        NVML_ERROR_INVALID_ARGUMENT,
        NVML_ERROR_NOT_SUPPORTED,
        NVML_ERROR_NO_PERMISSION,
        NVML_ERROR_ALREADY_INITIALIZED,
        NVML_ERROR_NOT_FOUND,
        NVML_ERROR_INSUFFICIENT_SIZE,
        NVML_ERROR_INSUFFICIENT_POWER,
        NVML_ERROR_DRIVER_NOT_LOADED,
        NVML_ERROR_TIMEOUT,
        NVML_ERROR_IRQ_ISSUE,
        NVML_ERROR_LIBRARY_NOT_FOUND,
        NVML_ERROR_FUNCTION_NOT_FOUND,
        NVML_ERROR_CORRUPTED_INFOROM,
        NVML_ERROR_GPU_IS_LOST,
        NVML_ERROR_RESET_REQUIRED,
        NVML_ERROR_OPERATING_SYSTEM,
        NVML_ERROR_LIB_RM_VERSION_MISMATCH,
        NVML_ERROR_IN_USE,
        NVML_ERROR_MEMORY,
        NVML_ERROR_NO_DATA,
        NVML_ERROR_VGPU_ECC_NOT_SUPPORTED,
        NVML_ERROR_INSUFFICIENT_RESOURCES,
        NVML_ERROR_UNKNOWN = 999
    }

    /// <summary>
    /// 显卡使用率信息模型
    /// </summary>
    public struct NvmlUtilization
    {
        public uint gpu { get; }
        public uint memory { get; }
    }
}
