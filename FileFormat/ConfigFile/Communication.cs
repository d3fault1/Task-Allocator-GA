namespace FileFormat
{
    /// <summary>
    /// Shows energy consumption when tasks communicate with each other
    /// </summary>
    public class Communication
    {

        public string Map { get; set; }

        public float[,] Matrix = null;

        int RowCount = 0;
        int ColumnCount = 0;

        /// <summary>
        /// Rows x Columns
        /// </summary>
        /// <param name="ProcessorCount"></param>
        /// <param name="TaskCount"></param>
        public void Init()
        {

            //MAP=0,0.0001,0.0001,0.0001,0.0001;0,0,0.00005,0,0;0,0,0,0.00005,0;0,0,0,0,0.00005;0.0001,0,0,0,0
            //Semicolon separates sections

            // Local communication costs (in terms of energy) for tasks 
            // executing on the same processor. A value of 0 means no communication between the two local tasks.
            // The ith section of a map (semicolon separated sections) relates to the ith task (sender).
            // The jth element in a section refers to the jth task (receiver).
            // Element i,j is the energy required for task i to locally communicate with task j.


            //Note:- Row shows energy consumed by task in row if it communicates with task at column

            var rows = Map.Split(';');
            var cols = rows[0].Split(',');

            Matrix = new float[rows.Length, cols.Length];
            RowCount = rows.Length;
            ColumnCount = cols.Length;


            int iRow = 0;
            foreach (var strRow in rows)
            {

                var strCols = strRow.Split(',');
                for (int icol = 0; icol < strCols.Length; icol++)
                {
                    if (float.TryParse(strCols[icol].Trim(), out var iVal))
                    {
                        Matrix[iRow, icol] = iVal;
                    }
                }
                iRow++;
            }

        }

        /// <summary>
        /// Gets communication energy consumption of a given task
        /// </summary>
        /// <param name="TaskId"></param>
        /// <returns></returns>
        public float GetEnergyConsumption(int TaskId)
        {
            float EnergySum = 0.0f;

            for (int toTaskId = 0; toTaskId < ColumnCount; toTaskId++)
            {
                EnergySum += Matrix[TaskId, toTaskId];
            }

            return EnergySum;
        }






    }



}
