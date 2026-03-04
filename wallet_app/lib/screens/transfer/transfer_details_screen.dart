import 'package:flutter/material.dart';
import '../../theme.dart';
import '../../services/api_service.dart';
import 'transfer_success_screen.dart';

class TransferDetailsScreen extends StatefulWidget {
  final String fromWalletId;
  final Map<String, dynamic> recipient;

  const TransferDetailsScreen({
    super.key,
    required this.fromWalletId,
    required this.recipient,
  });

  @override
  State<TransferDetailsScreen> createState() => _TransferDetailsScreenState();
}

class _TransferDetailsScreenState extends State<TransferDetailsScreen> {
  final _amountController = TextEditingController();
  final _noteController = TextEditingController();
  bool _showConfirm = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Add Account Details')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: _showConfirm
            ? _ConfirmView(
          fromWalletId: widget.fromWalletId,
          recipient: widget.recipient,
          amount: double.tryParse(_amountController.text) ?? 0,
          note: _noteController.text,
          onBack: () => setState(() => _showConfirm = false),
        )
            : Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _infoRow('Recipient', widget.recipient['fullName'] ?? ''),
            _infoRow('Bank', 'Digital Wallet'),
            const SizedBox(height: 20),
            const Text('Enter Amount',
                style: TextStyle(fontWeight: FontWeight.w600, fontSize: 15, color: AppColors.textSecondary)),
            const SizedBox(height: 8),
            TextField(
              controller: _amountController,
              keyboardType: const TextInputType.numberWithOptions(decimal: true),
              decoration: const InputDecoration(prefixText: '\$ ', hintText: '0.00'),
            ),
            const SizedBox(height: 16),
            const Text('Note (optional)',
                style: TextStyle(fontWeight: FontWeight.w600, fontSize: 15, color: AppColors.textSecondary)),
            const SizedBox(height: 8),
            TextField(
              controller: _noteController,
              decoration: const InputDecoration(hintText: 'Payment note...'),
            ),
            const SizedBox(height: 32),
            ElevatedButton(
              onPressed: () {
                if (_amountController.text.isEmpty) return;
                setState(() => _showConfirm = true);
              },
              child: const Text('Continue'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _infoRow(String label, String value) => Padding(
    padding: const EdgeInsets.only(bottom: 12),
    child: Row(children: [
      Text('$label: ', style: const TextStyle(color: AppColors.textSecondary)),
      Text(value, style: const TextStyle(fontWeight: FontWeight.w600)),
    ]),
  );
}

class _ConfirmView extends StatefulWidget {
  final String fromWalletId;
  final Map<String, dynamic> recipient;
  final double amount;
  final String note;
  final VoidCallback onBack;

  const _ConfirmView({
    required this.fromWalletId,
    required this.recipient,
    required this.amount,
    required this.note,
    required this.onBack,
  });

  @override
  State<_ConfirmView> createState() => _ConfirmViewState();
}

class _ConfirmViewState extends State<_ConfirmView> {
  final _otpController = TextEditingController();
  bool _isLoading = false;
  String? _error;

  Future<void> _send() async {
    final otp = _otpController.text.trim();
    if (otp.isEmpty) {
      setState(() => _error = 'Enter OTP');
      return;
    }
    final recipientWalletId =
        widget.recipient['walletId'] ?? widget.recipient['defaultWalletId'] ?? '';
    if (recipientWalletId.isEmpty) {
      setState(() => _error = 'Recipient wallet not found');
      return;
    }
    setState(() { _isLoading = true; _error = null; });
    try {
      final result = await ApiService.sendTransfer(
        fromWalletId: widget.fromWalletId,
        toWalletId: recipientWalletId,
        amount: widget.amount,
        otp: otp,
        note: widget.note.isNotEmpty ? widget.note : null,
      );
      if (!mounted) return;
      if (result['success'] == true || result['statusCode'] == 200 || result['data'] != null) {
        Navigator.pushReplacement(context, MaterialPageRoute(
          builder: (_) => TransferSuccessScreen(
            recipient: widget.recipient,
            amount: widget.amount,
            transactionData: result['data'] ?? result,
          ),
        ));
      } else {
        setState(() { _error = result['message'] ?? 'Transfer failed'; _isLoading = false; });
      }
    } catch (e) {
      setState(() { _error = e.toString(); _isLoading = false; });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Column(children: [
      const Text('Are you sure?',
          style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
      const SizedBox(height: 8),
      const Text('Please review your transfer details before confirming.',
          style: TextStyle(color: AppColors.textSecondary), textAlign: TextAlign.center),
      const SizedBox(height: 24),
      Container(
        padding: const EdgeInsets.all(16),
        decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(14)),
        child: Column(children: [
          _row('Recipient', widget.recipient['fullName'] ?? ''),
          _row('Amount', '\$${widget.amount.toStringAsFixed(2)}'),
          _row('Fee', '\$0.00'),
          const Divider(),
          _row('Total', '\$${widget.amount.toStringAsFixed(2)}', bold: true),
        ]),
      ),
      const SizedBox(height: 20),
      TextField(
        controller: _otpController,
        keyboardType: TextInputType.number,
        decoration: const InputDecoration(
            hintText: 'Enter OTP code',
            prefixIcon: Icon(Icons.lock_outline)),
      ),
      if (_error != null) ...[
        const SizedBox(height: 10),
        Text(_error!, style: const TextStyle(color: AppColors.error)),
      ],
      const SizedBox(height: 20),
      ElevatedButton(
        onPressed: _isLoading ? null : _send,
        child: _isLoading
            ? const SizedBox(height: 22, width: 22,
            child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
            : const Text('Send'),
      ),
      const SizedBox(height: 12),
      TextButton(onPressed: widget.onBack, child: const Text('Cancel')),
    ]);
  }

  Widget _row(String label, String value, {bool bold = false}) => Padding(
    padding: const EdgeInsets.symmetric(vertical: 6),
    child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
      Text(label, style: const TextStyle(color: AppColors.textSecondary)),
      Text(value, style: TextStyle(fontWeight: bold ? FontWeight.bold : FontWeight.w500)),
    ]),
  );
}