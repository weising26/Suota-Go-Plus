using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    internal class FileManager : BindableBase, IFileManager
    {
        private IEventAggregator _aggregator;
        private ILoggerFacade _logger;
        private INotifyManager _notifyManager;
        private IStateManager _stateManager;

        /// <summary>
        /// Firmware search directory (e.g. /storage/emulated/0/SuotaPgp).
        /// </summary>
        private readonly string _path;

        private byte[] _firmware;

        private byte _crc;
        public byte Crc
        {
            get => _crc;
            set => SetProperty(ref _crc, value);
        }

        private int _fileSize;
        public int FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        private byte[] _patch;
        public byte[] Patch
        {
            get => _patch;
            private set => SetProperty(ref _patch, value);
        }

        private byte[] _header;
        public byte[] Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private int _numOfBlocks;
        public int NumOfBlocks
        {
            get => _numOfBlocks;
            private set => SetProperty(ref _numOfBlocks, value);
        }

        public FileManager(IEventAggregator aggregator,
                           ILoggerFacade logger,
                           INotifyManager notifyManager,
                           IStateManager stateManager)
        {
            _aggregator = aggregator;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;
            _path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath,
                                 Constants.AppFolderName);

            Patch = new byte[Constants.PatchLength];
            Header = new byte[Constants.HeaderSize];

            if (!stateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized))
            {
                CreateAppFolder();
            }
        }

        public void LoadFirmware(string fileName)
        {
            _firmware = null;

            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".img"))
            {
                _logger.Log("Unable to load firmware file, invalid filename or extension.", Category.Exception, Priority.High);
                return;
            }

            _logger.Log("Attempting to load firmware file", Category.Info, Priority.None);

            string filePath = Path.Combine(_path, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.Log("Unable to load firmware file, file does not exist.", Category.Exception, Priority.High);
                return;
            }

            _logger.Log("Reading file...", Category.Info, Priority.None);
            try
            {
                _firmware = System.IO.File.ReadAllBytes(filePath);

                FileSize = _firmware.Length;

                _logger.Log("Read file complete", Category.Info, Priority.None);

                NumOfBlocks = FileSize / Constants.BlockSize +
                            ((FileSize % Constants.BlockSize == 0) ? 0 : 1);

                for (int i = 0; i < Constants.PatchLength; i++)
                {
                    Patch[i] = _firmware[i + 2];
                }

                for (int i = 0; i < Constants.HeaderSize; i++)
                {
                    Header[i] = _firmware[i];
                }

                int imageSize = (_firmware[7] << 24) | (_firmware[6] << 16) |
                                (_firmware[5] << 8) | (_firmware[4]);

                imageSize += 6;

                for (int i = 0; i < 4; i++)
                {
                    _firmware[4 + i] = (byte)((imageSize >> (8 * i)) & 0xFF);
                }

                CalculateCRC();
            }
            catch (Exception e)
            {
                _logger.Log($"Unable to read file. Reason: {e.Message}", Category.Exception, Priority.High);
                _firmware = null;
                FileSize = 0;
                NumOfBlocks = 0;
            }
        }

        public List<byte[]> GetChunks(int blockIndex)
        {
            int startIndex = blockIndex * Constants.BlockSize;

            int blockSize;

            if (startIndex + Constants.BlockSize > FileSize)
            {
                blockSize = FileSize - startIndex;
            }
            else
            {
                blockSize = Constants.BlockSize;
            }

            int numOfChunks = blockSize / Constants.ChunkSize +
                            ((blockSize % Constants.ChunkSize == 0) ? 0 : 1);

            List<byte[]> chunks = new List<byte[]>(numOfChunks);

            int bytesLeft = blockSize;

            for (int i = 0; i < numOfChunks; i++)
            {
                if (bytesLeft >= Constants.ChunkSize)
                {
                    chunks.Add(new byte[Constants.ChunkSize]);
                    bytesLeft -= Constants.ChunkSize;
                }
                else
                {
                    chunks.Add(new byte[bytesLeft]);
                    bytesLeft = 0;
                }

                for (int j = 0; j < chunks[i].Length; j++)
                {
                    int index = startIndex + (i * Constants.ChunkSize) + j;
                    chunks[i][j] = _firmware[index];
                }
            }

            return chunks;
        }

        public List<byte[]> GetHeaderChunks()
        {
            int blockSize = Constants.HeaderSize;

            int numOfChunks = blockSize / Constants.ChunkSize +
                            ((blockSize % Constants.ChunkSize == 0) ? 0 : 1);

            List<byte[]> chunks = new List<byte[]>(numOfChunks);

            int bytesLeft = blockSize;

            for (int i = 0; i < numOfChunks; i++)
            {
                if (bytesLeft >= Constants.ChunkSize)
                {
                    chunks.Add(new byte[Constants.ChunkSize]);
                    bytesLeft -= Constants.ChunkSize;
                }
                else
                {
                    chunks.Add(new byte[bytesLeft]);
                    bytesLeft = 0;
                }

                for (int j = 0; j < chunks[i].Length; j++)
                {
                    int index = (i * Constants.ChunkSize) + j;
                    chunks[i][j] = _header[index];
                }
            }

            return chunks;
        }

        public Task<List<PatchFile>> GetFirmwareFileNames()
        {
            if (_stateManager.State != AppState.Idle)
                return Task.FromResult<List<PatchFile>>(null);

            _stateManager.State = AppState.Loading;

            _logger.Log("Attempting to get firmware File names", Category.Info, Priority.None);

            List<PatchFile> fileNames = new List<PatchFile>();

            try
            {
                if (!Directory.Exists(_path))
                {
                    CreateAppFolder();
                }

                if (!Directory.Exists(_path))
                {
                    _notifyManager.ShowShortToast("Storage inaccessible. Please make sure that storage permissions are enabled.");
                    return Task.FromResult<List<PatchFile>>(null);
                }

                foreach (var file in Directory.GetFiles(_path, "*.img"))
                {
                    fileNames.Add(new PatchFile() { Name = Path.GetFileName(file) });
                }

                if (fileNames.Count == 0)
                {
                    _notifyManager.ShowShortToast($"No files found. Please make sure the firmware file is in the '{Constants.AppFolderName}' folder.");
                }
            }
            catch (Exception e)
            {
                _logger.Log($"Error enumerating firmware files: {e.Message}", Category.Exception, Priority.High);
                _notifyManager.ShowShortToast("Storage inaccessible. Please make sure that storage permissions are enabled.");
                return Task.FromResult<List<PatchFile>>(null);
            }
            finally
            {
                _stateManager.State = AppState.Idle;
            }

            return Task.FromResult(fileNames);
        }

        public async void Save(GoPlus device)
        {
            _logger.Log("Attempting to Save DeviceInfo", Category.Info, Priority.None);

            if (device == null)
            {
                _logger.Log("DeviceInfo was null", Category.Exception, Priority.High);
                return;
            }

            try
            {
                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                    _logger.Log("Created \"" + Constants.AppFolderName + "\" directory", Category.Info, Priority.None);
                }
            }
            catch
            {
                _logger.Log("Unable to create \"" + Constants.AppFolderName + "\" directory", Category.Exception, Priority.High);
                _notifyManager.ShowShortToast("Unable to write key file. Make sure you have storage permissions enabled.");
                return;
            }

            string json = Serialize(device);

            string fileName = Constants.AppFileName + " " + device.BtAddress + " " +
                              DateTime.Now.ToString().Replace(':', '-').Replace('/', '-') +
                              ".json";

            string fullPath = Path.Combine(_path, fileName);

            try
            {
                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    _logger.Log("Writing to file \"" + fileName + "\"", Category.Info, Priority.None);
                    await sw.WriteAsync(json);
                    _notifyManager.ShowShortToast($"Saved to {fullPath}");
                }
            }
            catch
            {
                _notifyManager.ShowShortToast("Unable to write key file. Make sure you have storage permissions enabled.");
                _logger.Log("Error writing to file \"" + fileName + "\"", Category.Exception, Priority.None);
            }
        }

        private string Serialize(GoPlus device)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"bluetooth\": \"");
            sb.Append(device.BtAddress);
            sb.Append("\",\n");
            sb.Append("  \"device\": \"");
            sb.Append(device.DeviceKey);
            sb.Append("\",\n");
            sb.Append("  \"blob\": \"");
            sb.Append(device.BlobKey);
            sb.Append("\"\n}");
            return sb.ToString();
        }

        private void CreateAppFolder()
        {
            try
            {
                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                    _logger.Log("Created \"" + Constants.AppFolderName + "\" directory", Category.Info, Priority.None);
                }
            }
            catch
            {
                _logger.Log("Unable to create \"" + Constants.AppFolderName + "\" directory", Category.Exception, Priority.High);
            }
        }

        private void CalculateCRC()
        {
            byte crc = 0;

            for (int i = 0; i < _firmware.Length; i++)
            {
                crc ^= _firmware[i];
            }

            for (int i = 0; i < Constants.PatchLength; i++)
            {
                crc ^= Patch[i];
            }

            Crc = crc;
        }
    }
}
