using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Win32;

namespace CopyMerge.Services
{
    public static class AutorunManager
    {
        private const string APP_NAME = "CopyMerge";
        
        private const string RUN_KEY_PATH = @"Software\Microsoft\Windows\CurrentVersion\Run";
        
        /// <summary>
        /// Включает или отключает автозапуск приложения
        /// </summary>
        /// <param name="enable">true - включить автозапуск, false - отключить</param>
        /// <returns>Результат операции с описанием</returns>
        public static AutorunResult SetAutorun(bool enable)
        {
            try
            {
                string executablePath = GetExecutablePath();
                if (string.IsNullOrEmpty(executablePath))
                {
                    return new AutorunResult 
                    { 
                        Success = false, 
                        ErrorMessage = "Не удалось определить путь к исполняемому файлу" 
                    };
                }

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, true))
                {
                    if (key == null)
                    {
                        return new AutorunResult 
                        { 
                            Success = false, 
                            ErrorMessage = "Не удалось открыть ключ реестра для автозапуска" 
                        };
                    }

                    if (enable)
                    {
                        string quotedPath = $"\"{executablePath}\"";
                        key.SetValue(APP_NAME, quotedPath, RegistryValueKind.String);
                        
                        
                        return new AutorunResult 
                        { 
                            Success = true, 
                            Message = "Автозапуск успешно включен" 
                        };
                    }
                    else
                    {
                        if (key.GetValue(APP_NAME) != null)
                        {
                            key.DeleteValue(APP_NAME, false);
                        }
                        
                        return new AutorunResult 
                        { 
                            Success = true, 
                            Message = "Автозапуск успешно отключен" 
                        };
                    }
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                string errorMsg = "Недостаточно прав для изменения автозапуска. Запустите приложение от имени администратора.";
                
                return new AutorunResult 
                { 
                    Success = false, 
                    ErrorMessage = errorMsg,
                    RequiresElevation = true
                };
            }
            catch (Exception ex)
            {
                return new AutorunResult 
                { 
                    Success = false, 
                    ErrorMessage = $"Ошибка при настройке автозапуска: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Проверяет, включен ли автозапуск приложения
        /// </summary>
        /// <returns>true - автозапуск включен, false - отключен или ошибка проверки</returns>
        public static bool IsAutorunEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, false))
                {
                    if (key == null)
                    {
                        return false;
                    }

                    string registryValue = key.GetValue(APP_NAME) as string;
                    bool isEnabled = !string.IsNullOrEmpty(registryValue);
                    return isEnabled;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Синхронизирует настройки приложения с реальным состоянием автозапуска в реестре
        /// </summary>
        /// <param name="currentSetting">Текущая настройка автозапуска в приложении</param>
        /// <returns>Результат синхронизации</returns>
        public static AutorunSyncResult SynchronizeWithRegistry(bool currentSetting)
        {
            try
            {
                bool registryState = IsAutorunEnabled();
                
                if (registryState == currentSetting)
                {
                    return new AutorunSyncResult
                    {
                        Success = true,
                        SynchronizationNeeded = false,
                        CurrentState = registryState,
                        Message = "Настройки синхронизированы"
                    };
                }

                var setResult = SetAutorun(currentSetting);
                
                return new AutorunSyncResult
                {
                    Success = setResult.Success,
                    SynchronizationNeeded = true,
                    CurrentState = currentSetting,
                    Message = setResult.Success 
                        ? "Реестр синхронизирован с настройками приложения" 
                        : setResult.ErrorMessage,
                    ErrorMessage = setResult.Success ? null : setResult.ErrorMessage
                };
            }
            catch (Exception ex)
            {
                return new AutorunSyncResult
                {
                    Success = false,
                    SynchronizationNeeded = false,
                    CurrentState = currentSetting,
                    ErrorMessage = $"Ошибка при синхронизации: {ex.Message}"
                };
            }
        }
        public static AutorunInfo GetAutorunInfo()
        {
            try
            {
                bool isEnabled = IsAutorunEnabled();
                string executablePath = GetExecutablePath();
                string registryPath = null;

                if (isEnabled)
                {
                    using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RUN_KEY_PATH, false))
                    {
                        registryPath = key?.GetValue(APP_NAME) as string;
                    }
                }

                return new AutorunInfo
                {
                    IsEnabled = isEnabled,
                    ExecutablePath = executablePath,
                    RegistryPath = registryPath,
                    RegistryKey = $@"HKEY_CURRENT_USER\{RUN_KEY_PATH}",
                    ValueName = APP_NAME
                };
            }
            catch (Exception ex)
            {
                return new AutorunInfo
                {
                    IsEnabled = false,
                    ErrorMessage = ex.Message
                };
            }
        }


        private static string GetExecutablePath()
        {
            try
            {
                return Assembly.GetExecutingAssembly().Location;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

    }
}