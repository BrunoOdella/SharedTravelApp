﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.BL
{
    public class Calificacion
    {
        private int _userId;
        private int _tripId;

        public float Score { get; set; }
        public string Comment { get; set; }

        public Calificacion(int userId, int tripId, float score, string comment)
        {
            this._userId = userId;
            this._tripId = tripId;
            this.Score = score;
            this.Comment = comment;
        }

    }
} 