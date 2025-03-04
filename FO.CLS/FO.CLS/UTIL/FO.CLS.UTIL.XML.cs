using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FO.CLS.LOG;

namespace FO.CLS.UTIL
{
    public class XML
    {
        #region 상수 및 변수

        private const string DEFAULT_ELEMENT = "fourone";

        private const string DEFAULT_NODE = "/fourone";

        // XML 설정 기본 디렉토리 경로
        public const string DEFAULT_CONFIG_DIRECTORY = @"\Config";

        // XML 설정 기본 파일명
        public const string DEFAULT_CONFIG_FILE_NAME = @"\AppConfig.xml";

        // XML 백업 파일명
        public const string DEFAULT_BACKUP_FILE_NAME = @"\Backup.xml";

        // 파일 이름
        private string FileNameString = string.Empty;

        // 파일 경로
        private string FilePathString = string.Empty;

        // 딕셔너리 선언
        private Dictionary<string, string> DictionarySettings;

        // 로그 클래스 선언
        private Write FOCLSLOGWrite = new Write(null);
        #endregion

        #region 생성자

        private void copyConfig(string pathFrom, string pathTo)
        {
            try
            {
                File.Copy(pathFrom, pathTo, true);
            }
            catch
            {

            }
        }

        private void restoreBackup()
        {
            string pathFrom =  @Directory.GetCurrentDirectory() + DEFAULT_CONFIG_DIRECTORY + DEFAULT_BACKUP_FILE_NAME;
            string pathTo =  @Directory.GetCurrentDirectory() + DEFAULT_CONFIG_DIRECTORY + DEFAULT_CONFIG_FILE_NAME;

            copyConfig(pathFrom, pathTo);
        }

        private void saveBackup()
        {
            string pathFrom =  @Directory.GetCurrentDirectory() + DEFAULT_CONFIG_DIRECTORY + DEFAULT_BACKUP_FILE_NAME;
            string pathTo =  @Directory.GetCurrentDirectory() + DEFAULT_CONFIG_DIRECTORY + DEFAULT_CONFIG_FILE_NAME;

            copyConfig(pathTo, pathFrom);
        }

        public XML()
        {
            DictionarySettings = new Dictionary<string, string>();

            // 파일 경로 설정
            FilePathString = @Directory.GetCurrentDirectory() + DEFAULT_CONFIG_DIRECTORY;

            // 디렉토리 유무 -> 디렉토리 생성 
            if(!Directory.Exists(FilePathString))
            {
                Directory.CreateDirectory(FilePathString);
            }

            restoreBackup();

            // 파일 이름 설정
            FileNameString = FilePathString + DEFAULT_CONFIG_FILE_NAME;

            // 파일 유무
            if(!File.Exists(FileNameString))
            {
                CreatXMLFile();
            }
            else
            {
                LoadXMLFile();
            }
        }

        #endregion

        #region 메서드
        // XML 파일 생성
        private void CreatXMLFile()
        {
            try
            {
                XmlTextWriter xmlTextWriter = new XmlTextWriter(FileNameString, null);

                xmlTextWriter.Formatting = Formatting.Indented;

                // XML 작성 시작
                xmlTextWriter.WriteStartDocument();

                // 시작 태그 작성
                xmlTextWriter.WriteStartElement(DEFAULT_ELEMENT);

                IDictionaryEnumerator dictionaryEnumerator = DictionarySettings.GetEnumerator();

                while(dictionaryEnumerator.MoveNext())
                {
                    // Element 쓰기
                    xmlTextWriter.WriteElementString(dictionaryEnumerator.Key.ToString(), dictionaryEnumerator.Value.ToString());
                }

                // Element 닫기
                xmlTextWriter.WriteEndElement();

                // XML 작성 End 
                xmlTextWriter.WriteEndDocument();

                // 스트림 플러쉬
                xmlTextWriter.Flush();

                // 스트림 닫기
                xmlTextWriter.Close();
            }
            catch(Exception ex)
            {
                FOCLSLOGWrite.WriteLog("CreatXMLFile - " + ex.ToString());
            }

        }

        // XML 파일 불러오기
        private void LoadXMLFile()
        {
            // XML 문서 선언
            XmlDocument xmlDocument = new XmlDocument();
            XmlNodeList xmlNodeList;

            try
            {
                // XML 로드
                xmlDocument.Load(FileNameString);

                xmlNodeList = xmlDocument.SelectNodes(DEFAULT_NODE);

                string key = string.Empty;
                string value = string.Empty;

                foreach(XmlNode node in xmlNodeList[0].ChildNodes)
                {
                    key = node.Name;
                    value = node.ChildNodes.Count > 0 ? node.ChildNodes[0].Value : string.Empty;

                    DictionarySettings.Add(key, value);
                }
            }
            catch(FileNotFoundException ex)
            {
                FOCLSLOGWrite.WriteLog("LoadXMLFile FileNotFoundException - " + ex.ToString());
            }
            catch(Exception ex)
            {
                FOCLSLOGWrite.WriteLog("LoadXMLFile Exception - " + ex.ToString());
            }
            finally
            {
                xmlDocument = null;
                xmlNodeList = null;
                GC.Collect();
            }
        }

        /// <summary>
        /// XML 파일 값 Read
        /// </summary>
        /// <param name="key">키</param>
        /// <param name="defaultValue">기본 값</param>
        /// <returns>읽어온 값</returns>
        public string ReadValue(string key, string defaultValue)
        {
            string value = string.Empty;

            // 키값에 해당하는 값 불러오기
            // 해당 값 없을 겨우 default 값 
            string readValue = DictionarySettings.TryGetValue(key, out value) ? value : defaultValue;

            return readValue;
        }

        /// <summary>
        /// XML 파일 값 Write
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool WriteValue(string key, string value)
        {
            try
            {
                // 이미 저장된 값 확인
                string checkValue = string.Empty;
                bool existValueFlag = DictionarySettings.TryGetValue(key, out checkValue);

                if(!existValueFlag)
                {
                    // 기존에 저장된 값 없으면 신규 추가
                    DictionarySettings.Add(key, value);
                    SaveXML(key, value);
                }
                else if(!checkValue.Equals(value))
                {
                    // 기존에 저장된 값이랑 다르면 변경 
                    DictionarySettings[key] = value;
                    SaveXML(key, value);
                }

                saveBackup();

                return true;
            }
            catch(Exception ex)
            {
                FOCLSLOGWrite.WriteLog("WriteValue Exception - " + ex.ToString());

                return false;
            }
        }

        /// <summary>
        /// XML 파일에 값 저장
        /// </summary>
        /// <param name="key">키</param>
        /// <param name="value">값</param>
        private void SaveXML(string key, string value)
        {
            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(FileNameString);

                string xPath = string.Format("{0}/{1}", DEFAULT_NODE, key);
                XmlNodeList xmlNodeList = xmlDocument.SelectNodes(xPath);

                if(xmlNodeList.Count == 0)
                {
                    // 신규 추가
                    // 지정된 이름으로 생성
                    XmlNode xmlNode = xmlDocument.CreateElement(key);

                    // 지정된 노드를 선택된 노드의 자식 노드 목록에 추가
                    xmlDocument.SelectSingleNode(DEFAULT_NODE).AppendChild(xmlNode);

                    XmlText xmlText = xmlDocument.CreateTextNode(value);

                    // 노드에 값 추가
                    xmlNode.AppendChild(xmlText);
                }
                else
                {
                    // 값 변경
                    if(xmlNodeList[0].ChildNodes.Count > 0)
                    {
                        // 노드에 키 값이 있는 경우 값 변경
                        xmlNodeList[0].FirstChild.Value = value;
                    }
                    else
                    {
                        // 없으면 노드 추가
                        XmlText xmlText = xmlDocument.CreateTextNode(value);

                        xmlNodeList[0].AppendChild(xmlText);
                    }
                }

                xmlDocument.Save(FileNameString);
            }
            catch(Exception ex)
            {
                FOCLSLOGWrite.WriteLog("SaveXML Exception - " + ex.ToString());
            }
        }
        #endregion
    }
}
