using System;
using System.Diagnostics;

namespace VSTP.Video2Audio
{
    class VideoToAudio
    {
        private string videoUrl;
        private string targetUrl;
        private string ffmpegUrl;
        private Process process;

        public VideoToAudio()
        {
            ffmpegUrl = @"ffmpeg.exe"; //这是一个很普通的默认情况，假设我们的转码工具也在根目录下
        }

        /// <summary>
        /// 更改转码工具ffmpeg.exe的路径
        /// 默认状态是ffmpeg.exe在根目录下
        /// </summary>
        /// <param name="url">ffmpeg.exe的绝对路径</param>
        public void Setffmpeg(string url)
        {
            ffmpegUrl = url;
        }

        /// <summary>
        /// 设置视频和音频的输入/输出路径
        /// 无默认值，要手动设置
        /// </summary>
        /// <param name="urlVideo">视频输入路径</param>
        /// <param name="urlAudio">音频输出路径</param>
        public void SetUrl(string urlVideo, string urlAudio)
        {
            videoUrl = urlVideo;
            targetUrl = urlAudio;
        }

        /// <summary>
        /// 开始转换，自动阻塞
        /// </summary>
        public void Trans()
        {
            process = new Process();
            process.StartInfo.FileName = ffmpegUrl;
            var para = string.Format("-y -i {0} -acodec pcm_s16le -f s16le -ac 1 -ar 16000 {1}", videoUrl, targetUrl);
            process.StartInfo.Arguments = para;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.WaitForExit();
            process.Close();
        }
    }
}
