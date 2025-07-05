using UnityEngine;
using System.IO;
using System.Text;

public class DataLogger : MonoBehaviour
{
    // ���ô˺�������¼���ݣ�ʱ�䲽�ͱ���ֵ��
    public void LogData(float timeStep, float variableValue)
    {
        string fileName = "DataLog_rvo.csv";
        string filePath = Path.Combine(Application.dataPath, fileName);

        // ����CSV��ʽ��������
        string newLine = $"{timeStep},{variableValue}";

        // ����ļ��Ƿ���ڲ�������ͷ
        bool needHeader = !File.Exists(filePath);

        // ʹ��׷��ģʽд���ļ�
        using (StreamWriter writer = new StreamWriter(filePath, true, Encoding.UTF8))
        {
            if (needHeader)
            {
                writer.WriteLine("TimeStep,Value"); // д���б���
            }
            writer.WriteLine(newLine);
        }
    }
}