﻿using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
	public class BrandModel
	{
		[Key]
		public long Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên thương hiệu")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập mô tả thương hiệu")]
		public string Description { get; set; }
		public string Slug { get; set; }
		public int Status { get; set; }
	}
}
