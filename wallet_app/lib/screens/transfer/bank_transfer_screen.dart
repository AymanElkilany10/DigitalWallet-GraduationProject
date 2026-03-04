import 'package:flutter/material.dart';
import '../../theme.dart';
import '../../services/api_service.dart';
import 'transfer_details_screen.dart';

class BankTransferScreen extends StatefulWidget {
  const BankTransferScreen({super.key});

  @override
  State<BankTransferScreen> createState() => _BankTransferScreenState();
}

class _BankTransferScreenState extends State<BankTransferScreen> {
  final _emailController = TextEditingController();
  bool _isLoading = false;
  String? _error;
  Map<String, dynamic>? _foundUser;
  List<dynamic> _myWallets = [];
  String? _selectedWalletId;

  @override
  void initState() {
    super.initState();
    _loadWallets();
  }

  Future<void> _loadWallets() async {
    final data = await ApiService.getMyWallets();
    setState(() {
      _myWallets = data['data'] ?? data['wallets'] ?? [];
      if (_myWallets.isNotEmpty) {
        _selectedWalletId = _myWallets[0]['id']?.toString();
      }
    });
  }

  Future<void> _searchUser() async {
    final email = _emailController.text.trim();
    if (email.isEmpty) return;
    setState(() { _isLoading = true; _error = null; _foundUser = null; });
    try {
      final data = await ApiService.findUserByEmail(email);
      setState(() {
        _foundUser = data['data'] ?? data;
        _isLoading = false;
      });
    } catch (e) {
      setState(() { _error = 'User not found'; _isLoading = false; });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('🚀 Bank Transfer')),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text('From Wallet',
                style: TextStyle(fontWeight: FontWeight.w600, fontSize: 15, color: AppColors.textSecondary)),
            const SizedBox(height: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: AppColors.divider),
              ),
              child: DropdownButtonHideUnderline(
                child: DropdownButton<String>(
                  value: _selectedWalletId,
                  isExpanded: true,
                  hint: const Text('Select wallet'),
                  items: _myWallets.map<DropdownMenuItem<String>>((w) {
                    return DropdownMenuItem(
                      value: w['id']?.toString(),
                      child: Text('${w['walletNumber'] ?? w['id']} — \$${w['balance'] ?? '0.00'}'),
                    );
                  }).toList(),
                  onChanged: (v) => setState(() => _selectedWalletId = v),
                ),
              ),
            ),
            const SizedBox(height: 24),
            const Text('Recipient Email',
                style: TextStyle(fontWeight: FontWeight.w600, fontSize: 15, color: AppColors.textSecondary)),
            const SizedBox(height: 8),
            Row(children: [
              Expanded(
                child: TextField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(hintText: 'Enter recipient email'),
                ),
              ),
              const SizedBox(width: 10),
              ElevatedButton(
                onPressed: _isLoading ? null : _searchUser,
                style: ElevatedButton.styleFrom(minimumSize: const Size(70, 52)),
                child: _isLoading
                    ? const SizedBox(width: 20, height: 20,
                    child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                    : const Text('Find'),
              ),
            ]),
            if (_error != null) ...[
              const SizedBox(height: 12),
              Text(_error!, style: const TextStyle(color: AppColors.error)),
            ],
            if (_foundUser != null) ...[
              const SizedBox(height: 20),
              _RecipientCard(user: _foundUser!),
              const SizedBox(height: 24),
              ElevatedButton(
                onPressed: () {
                  if (_selectedWalletId == null) return;
                  Navigator.push(context, MaterialPageRoute(
                    builder: (_) => TransferDetailsScreen(
                      fromWalletId: _selectedWalletId!,
                      recipient: _foundUser!,
                    ),
                  ));
                },
                child: const Text('Continue'),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

class _RecipientCard extends StatelessWidget {
  final Map<String, dynamic> user;
  const _RecipientCard({required this.user});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: AppColors.primary.withOpacity(0.3)),
      ),
      child: Row(children: [
        CircleAvatar(
          backgroundColor: AppColors.primary.withOpacity(0.12),
          radius: 26,
          child: Text(
            (user['fullName'] ?? user['name'] ?? 'U')[0].toUpperCase(),
            style: const TextStyle(color: AppColors.primary, fontWeight: FontWeight.bold, fontSize: 20),
          ),
        ),
        const SizedBox(width: 14),
        Expanded(child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(user['fullName'] ?? user['name'] ?? '',
                style: const TextStyle(fontWeight: FontWeight.w600, fontSize: 16)),
            Text(user['email'] ?? '',
                style: const TextStyle(color: AppColors.textSecondary, fontSize: 13)),
          ],
        )),
        const Icon(Icons.check_circle, color: AppColors.success),
      ]),
    );
  }
}