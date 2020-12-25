using System;
using System.Collections.Generic;
using System.Linq;

namespace TestApp2.Models
{
    public class TesterModel
    {
        public struct TaskData
        {
            public int count;
            public int time;
        }

        public string lastName;
        public int sumCountTest;
        public List<TaskData> taskDatas;

        public TesterModel()
        {
            lastName = "ИТОГО";
            sumCountTest = 0;
            taskDatas = new List<TaskData>();
            for (int i = 0; i < 6; i++)
                taskDatas.Add(new TaskData());
        }

        public List<string> getTesterInfoList()
        {
            var resultList = new List<string>();
            resultList.Add(lastName);
            resultList.Add(sumCountTest.ToString());
            for (int i = 0; i < taskDatas.Count; i++)
            {
                resultList.Add(taskDatas[i].count.ToString());
            }
            resultList.Add(Math.Round(GetVolumeOfComplexity(),2).ToString());
            resultList.Add(Math.Round(GetPlannedTestingTime(), 2).ToString());
            resultList.Add(Math.Round(GetActualTestingTime(), 2).ToString());
            resultList.Add(Math.Round((GetPlannedTestingTime() - GetActualTestingTime()),2).ToString());

            return resultList;
        }

        public void AddTaskData(int comlexity, int time)
        {
            var val = taskDatas[comlexity-1];
            val.count += 1;
            val.time += time;
            taskDatas[comlexity-1] = val;
            sumCountTest += 1;
        }

        public double GetVolumeOfComplexity()
        {
            double sum_volume = 0;
            for (int i = 0; i < taskDatas.Count; i++)
            {
                sum_volume += (((double)(i + 1)) / 10 * taskDatas[i].count);
            }
            return sum_volume;
        }

        public double GetPlannedTestingTime()
        {
            int[] planing_time = new int[6] { 15, 30, 60, 132, 180, 230 };
            double sum_time = 0;
            for (int i = 0; i < planing_time.Count(); i++)
            {
                sum_time += (planing_time[i] * taskDatas[i].count);
            }
            return sum_time / 60;
        }

        public double GetActualTestingTime()
        {
            double sum_time = 0;
            for (int i = 0; i < taskDatas.Count; i++)
            {
                sum_time += (taskDatas[i].time);
            }
            return sum_time / 60;
        }

    }
}