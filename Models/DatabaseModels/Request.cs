﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Animal_Hotel.Models.DatabaseModels
{
    [Table("request")]
    public class Request
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(maximumLength:1000, ErrorMessage = "Request message length should be less than 1000 characters")]
        public string Text { get; set; }

        [Required]
        [Column("writing_date")]
        [DataType(DataType.Date)]
        public DateTime WritingDate { get; set; }

        [Required]
        [Column("status_id")]
        [ForeignKey("Status")]
        public short StatusId { get; set; }

        public RequestStatus Status { get; set; } = null!;

        [Required]
        [Column("employee_id")]
        [ForeignKey("Writer")]
        public long EmployeeId { get; set; }

        public Employee Writer { get; set; } = null!;

        public Request(string text, short statusId, long employeeId)
        {
            this.Text = text;
            this.StatusId = statusId;
            this.EmployeeId = employeeId;
        }
    }
}