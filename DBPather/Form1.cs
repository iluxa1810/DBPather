using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Xml;
using log4net;
using log4net.Config;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Access;
using Microsoft.Office.Interop.Access.Dao;
using Microsoft.Vbe.Interop;
using Application = Microsoft.Office.Interop.Access.Application;
using Form = System.Windows.Forms.Form;
using System.Configuration;
using DBPather.Properties;

namespace DBPather
{
    public partial class Form1 : Form//
    {
        public Form1()
        {
            InitializeComponent();
        }

        class Сorrection //Класс для хранения патча
        {
            public string Name { get; set; }
            public string Type { get; set; }//asdasdadad
            public string Action { get; set; }
            public string Command { get; set; }
        }
        public static readonly ILog log = LogManager.GetLogger(typeof(Form1));
        /// <summary>
        /// Извлечение патча из файла
        /// </summary>
        /// <param name="units"></param>
        /// <param name="pathToPatch"></param>
        /// <returns></returns>
        static void execPatсh(List<Сorrection> units, string pathToPatch)
        {
            XmlDocument xDoc = new XmlDocument();
            var tmp = File.ReadAllText(pathToPatch, Encoding.UTF8);
            xDoc.LoadXml(tmp);
            foreach (XmlNode node in xDoc.SelectNodes("//Correction"))
            {
                Сorrection сorrection = new Сorrection();
                foreach (XmlAttribute attr in node.Attributes)
                {
                    if (attr.Name == "name")
                    {
                        сorrection.Name = attr.Value;
                        continue;
                    }
                    if (attr.Name == "type")
                    {
                        сorrection.Type = attr.Value;
                        continue;
                    }
                    if (attr.Name == "action")
                    {
                        сorrection.Action = attr.Value;
                        continue;
                    }

                }
                сorrection.Command = node.InnerText;
                units.Add(сorrection);
            }
            if (units.Count == 0)
            {
                throw new Exception("Некорректный патч. Продолжение невозможно");
            }
        }
        /// <summary>
        /// Проверка корректности путей
        /// </summary>
        /// <param name="dbPath"></param>
        /// <param name="patchPath"></param>
        /// <param name="logBox"></param>
        /// <returns></returns>
        static void CheckPath(string dbPath, string patchPath)//Проверка корректности путей
        {
            if (dbPath == string.Empty || patchPath == string.Empty)
            {
                throw new Exception("Есть пустые пути");
            }
            if (!Path.IsPathRooted(dbPath) || !Path.IsPathRooted(patchPath))
            {
                throw new Exception("Есть некорректные пути");
            }
        }
        /// <summary>
        /// Проверяет требуется ли удалить элемент Access или нет.
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="сorrection"></param>
        /// <returns></returns>
        static bool IsDeleted(Dictionary<string, string> dict, Сorrection сorrection)
        {
            if ((dict.Count > 0) && dict.ContainsKey(сorrection.Name) && (сorrection.Action == "ADD" || сorrection.Action == "DELETE"))
            {
                return true;
            }
            return false;
        }

        static bool IsExecuted(Dictionary<string, string> dict, Сorrection сorrection)
        {
            if ((dict.Count > 0) && dict.ContainsKey(сorrection.Name) && (сorrection.Action == "EXECUTE"))
            {
                return true;
            }
            throw new Exception("Попытка выполнить обращение к несуществующему объекту");
        }
        /// <summary>
        /// Обработка запросов
        /// </summary>
        /// <param name="patch"></param>
        /// <param name="db"></param>
        /// <param name="logBox"></param>
        /// <returns></returns>
        static void ProcessingQueries(List<Сorrection> patch, Application app, RichTextBox logBox)//Обработка запросов
        {
            Database db = app.CurrentDb();
            Dictionary<string, string> queryDict = new Dictionary<string, string>();
            foreach (QueryDef query in db.QueryDefs)
            {
                queryDict.Add(query.Name, query.SQL);//Получаю список имеющихся запросов
                NAR(query);
            }
            foreach (var unit in patch)
            {
                try
                {
                    if (IsDeleted(queryDict, unit))
                    {
                        db.DeleteQueryDef(unit.Name);
                        queryDict.Remove(unit.Name);
                    }
                    if (unit.Action == "ADD")
                    {
                        db.CreateQueryDef(unit.Name, unit.Command);
                        queryDict.Add(unit.Name, unit.Command);
                        continue;
                    }
                    if (IsExecuted(queryDict, unit))
                    {
                        db.Execute(unit.Name.Replace("\n", string.Empty).Replace("\r", string.Empty));
                    }
                    AddMessage(logBox, $"Над {unit.Name} успешно выполнено действие {unit.Action}", 0);
                }
                catch (Exception ex)
                {
                    NAR(db);
                    AddMessage(logBox, $"При выполнении над: {unit.Name} действие {unit.Action} произошла ошибка: {ex}", 1);
                    throw ex;
                }
            }
            NAR(db);
        }
        /// <summary>
        /// Получает список процедур, которые содержаться в Модуле.
        /// </summary>
        /// <param name="module"></param>
        /// <param name="macroDict"></param>
        static void GetProc(VBComponent module, Dictionary<string, string> macroDict)
        {
            int lineNum = module.CodeModule.CountOfDeclarationLines + 1;
            vbext_ProcKind procKind;
            do
            {
                string procName = module.CodeModule.ProcOfLine[lineNum, out procKind];
                if (!macroDict.ContainsKey(module.Name))
                {
                    macroDict.Add(module.Name, procName);
                }
                if (procName == null)
                {
                    break;
                }
            }
            while (lineNum >= module.CodeModule.CountOfLines);
            NAR(procKind);
        }
        /// <summary>
        /// Обработка модулей
        /// </summary>
        /// <param name="patch">Патч</param>
        /// <param name="app">Приложение</param>
        /// <param name="logBox">RichTextBox для логирования</param>
        /// <returns></returns>
        static void ProcessingModules(List<Сorrection> patch, Application app, RichTextBox logBox)
        {
            VBProject project = app.VBE.ActiveVBProject;
            Dictionary<string, string> vbDict = new Dictionary<string, string>();
            //Dictionary<string, string> macroDict = new Dictionary<string, string>();
            foreach (VBComponent module in project.VBComponents)
            {
                string moduleText = module.CodeModule.Lines[1, module.CodeModule.CountOfLines];
                vbDict.Add(module.Name, moduleText);
                // GetProc(module, macroDict);
                NAR(module);
            }
            foreach (var unit in patch)
            {
                try
                {
                    if (IsDeleted(vbDict, unit))
                    {
                        project.VBComponents.Remove(project.VBComponents.Item(unit.Name));
                        vbDict.Remove(unit.Name);
                        if (unit.Action == "DELETE")
                        {
                            continue;
                        }
                    }
                    if (unit.Action == "ADD")
                    {
                        VBComponent module = project.VBComponents.Add(vbext_ComponentType.vbext_ct_StdModule);
                        module.Name = unit.Name;
                        module.CodeModule.AddFromString(unit.Command);
                        app.DoCmd.Save(AcObjectType.acModule, unit.Name);
                        vbDict.Add(unit.Name, unit.Command);
                        NAR(module);
                        continue;
                    }
                    if (IsExecuted(vbDict, unit))
                    {
                        app.DoCmd.SetWarnings(false);
                        object error = null;//Переменная для получения ошибок при выполнении модуля
                        app.Run(unit.Command.Replace("\n", string.Empty).Replace("\r", string.Empty), ref error);
                        if (error.ToString() != string.Empty)//Если не пусто, значит Модуль выполнился с ошибками
                        {
                            throw new Exception(error.ToString());
                        }
                        AddMessage(logBox, $"Над {unit.Name} успешно выполнено действие {unit.Action}", 0);
                    }
                }
                catch (Exception ex)
                {
                    NAR(project);
                    AddMessage(logBox, $"При выполнении над: {unit.Name} действие {unit.Action} произошла ошибка: ", 1);
                    throw ex;
                }
            }
            NAR(project);
        }
        /// <summary>
        /// Патчинг БД
        /// </summary>
        /// <param name="patch"></param>
        /// <param name="pathToDB"></param>
        /// <param name="logBox"></param>
        /// <returns></returns>
        static void PatchningDb(List<Сorrection> patch, string pathToDB, RichTextBox logBox)
        {
            Application app = new Application();
            app.Visible = false;
            app.Screen.Application.Visible = false;
            app.Application.UserControl = false;
            app.UserControl = false;
            app.OpenCurrentDatabase(pathToDB);
            app.AutomationSecurity = MsoAutomationSecurity.msoAutomationSecurityLow;
            AddMessage(logBox, "Подключился к БД", 0);
            try
            {
                SetRunUpdate(2);//Присваиваем, что база завершилась с ошибками, так как юзер может закрыть программу до завершения запросов, что приведт к не предвиденным последствиям
                if (patch.Any(x => x.Type == "SQL"))
                {
                    AddMessage(logBox, "Обновляю запросы", 0);
                    ProcessingQueries(patch.Where(x => x.Type == "SQL").ToList(), app, logBox);
                    AddMessage(logBox, "Обновление запросов успешно завершено", 0);
                }
                if (patch.Any(x => x.Type == "VBA"))
                {
                    AddMessage(logBox, "Обновляю модули", 0);
                    ProcessingModules(patch.Where(x => x.Type == "VBA").ToList(), app, logBox);
                    AddMessage(logBox, "Обновление модулей успешно завершено", 0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                app.CloseCurrentDatabase();
                app.Quit(AcQuitOption.acQuitSaveNone);
                NAR(app);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }
        static void NAR(object o)
        {
            try
            {
                Marshal.ReleaseComObject(o);
                o = null;
            }
            catch
            {
                o = null;
            }
        }
        /// <summary>
        /// Предварительно создает копию базы данных
        /// </summary>
        /// <param name="pathDb">Путь до БД, которую нужно забекапить</param>
        /// <param name="backUpPath">Путь к забекапленной БД</param>
        static void BackUpDataBase(string pathDb, ref string backUpPath)
        {
            string pathToBackUp = Path.Combine(Environment.CurrentDirectory, "BackUP");
            try
            {
                if (!Directory.Exists(pathToBackUp))
                {
                    Directory.CreateDirectory(pathToBackUp);
                }
                string newFile = Path.Combine(pathToBackUp, Path.GetFileName(pathDb));
                File.Copy(pathDb, newFile);
                if (CheckFiles(pathDb, newFile) == false)//Проверяем успешность созданного бекапа
                {
                    File.Delete(newFile);
                    backUpPath = string.Empty;
                    throw new Exception("Во время создания Бекапа произошла ошибка: Файлы не прошли проверку на равенство, продолжение невозможно");
                }
                backUpPath = newFile;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        static void RestoreDataBase(string oldDb, string backUpDb, RichTextBox logBox)
        {
            try
            {
                File.Copy(backUpDb, oldDb, true);
                if (CheckFiles(backUpDb, oldDb) == false)//На всякий случай проверяем успешность восстановления бекапа
                {
                    throw new Exception("Во время восстановления БД произошла ошибка: Файлы не прошли проверку на равенство");
                }
                AddMessage(logBox, "База успешно восстановлена", 0);
            }
            catch (Exception ex)
            {
                AddMessage(logBox, ex.ToString(), 1);
            }
        }
        /// <summary>
        /// Проверяем хешсумму
        /// </summary>
        /// <param name="oldFile"></param>
        /// <param name="newFile"></param>
        /// <returns></returns>
        static bool CheckFiles(string oldFile, string newFile)
        {
            byte[] firstHash;
            byte[] secondHash;
            using (var md5 = MD5.Create())
            {
                using (Stream f1 = File.OpenRead(oldFile))
                {
                    firstHash = md5.ComputeHash(f1);
                }
                using (Stream f2 = File.OpenRead(newFile))
                {
                    secondHash = md5.ComputeHash(f2);
                }
            }
            if (firstHash.Length != secondHash.Length)
            {
                return false;
            }
            for (int i = 0; i < firstHash.Length; i++)
            {
                if (firstHash[i] != secondHash[i])
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Проверяет и закрывает запущенные Access
        /// </summary>
        /// <param name="logBox"></param>
        static void CheckAccess(RichTextBox logBox)
        {
            var processes = Process.GetProcessesByName("MSACCESS");
            try
            {
                if (processes.Any())
                {
                    AddMessage(logBox, "Обнаружен запущенный ACCESS. Приложение будет закрыто", 0);
                    foreach (Process process in processes)
                    {
                        process.Kill();
                    }
                    AddMessage(logBox, "ACCESS успешно закрыт", 0);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// Удаляет файлы бекапа после успешного восстановления БД или успешного завершения программы
        /// </summary>
        /// <param name="logBox"></param>
        static void DeleteBackUp(RichTextBox logBox)
        {
            string pathToBackUp = Path.Combine(Environment.CurrentDirectory, "BackUP");
            try
            {
                if (Directory.GetFiles(pathToBackUp).Any())
                {
                    AddMessage(logBox, "Удаляю временные файлы", 0);
                    foreach (var file in Directory.GetFiles(pathToBackUp))
                    {
                        File.Delete(file);
                    }
                    AddMessage(logBox, "Временные файлы успешно удалены", 0);
                }
            }
            catch (Exception ex)
            {
                AddMessage(logBox, $"Во время удаления временных файлов произошла ошибка: {ex}", 1);
            }
        }
        /// <summary>
        /// Пишет сообщение в RichBox и в файл лога.
        /// </summary>
        /// <param name="logBox"></param>
        /// <param name="message"></param>
        /// <param name="logType">0-info;1-ошибка</param>
        static void AddMessage(RichTextBox logBox, string message, int logType)
        {
            logBox.AppendText($"\n{message}\n");
            logBox.Refresh();
            logBox.ScrollToCaret();
            if (logType == 0)
            {
                log.Info(message);
            }
            else
            {
                log.Error(message);
            }
        }

        static void SetRunUpdate(int status)
        {
            Settings.Default.lastRun = status;
            Settings.Default.Save();
        }

        static void CheckLastRun(RichTextBox logBox)
        {
            if (Settings.Default.lastRun == 2)
            {
                SetRunUpdate(0);
                throw new Exception("Во время последнего база не была восстановлена. Скопируйте ее из папки BackUp");
            }
            else
            {
                SetRunUpdate(0);
                DeleteBackUp(logBox);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {

            string backUpDb = string.Empty;
            logBox.Clear();
            AddMessage(logBox, "Начинаю обработку", 0);
            CheckAccess(logBox);
            try
            {
                CheckLastRun(logBox);//Проверка успешности последнего запуска
                CheckPath(textDbPath.Text, textPatchPath.Text);
                List<Сorrection> val = new List<Сorrection>();
                execPatсh(val, textPatchPath.Text);
                BackUpDataBase(textDbPath.Text, ref backUpDb);
                PatchningDb(val, textDbPath.Text, logBox);
                AddMessage(logBox, "Программа успешно завершена", 0);
                MessageBox.Show("Программа успешно завершена", "Success", MessageBoxButtons.OK);
                SetRunUpdate(0);
                if (backUpDb != string.Empty)
                {
                    DeleteBackUp(logBox);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Программа завершилась с ошибками. Подробности в окне с сообщениями", "Error", MessageBoxButtons.OK);
                AddMessage(logBox, $"Программа завершена с ошибками: {ex}", 1);
                if (backUpDb != string.Empty)//Если Бекап был создан
                {
                    try
                    {
                        RestoreDataBase(textDbPath.Text, backUpDb, logBox);
                        DeleteBackUp(logBox);
                        SetRunUpdate(1);//Программа завершилась с ошибками, но база была восстановлена
                    }
                    catch (Exception inEx)
                    {
                        AddMessage(logBox, inEx.ToString(), 1);
                        SetRunUpdate(2);//Программа завершилась с ошибками и база не восстановлена
                    }
                }
                SetRunUpdate(1);
            }
            finally
            {
                CheckAccess(logBox);//Закрываем Access
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Файлы баз данных (*.mdb)|*.mdb";
            fd.ShowDialog();
            textDbPath.Text = fd.FileName;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.Filter = "Файлы исправлений(*.pth)|*.pth";
            fd.ShowDialog();
            textPatchPath.Text = fd.FileName;
        }
    }
}
